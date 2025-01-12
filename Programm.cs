using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BonfireServer.Internal;
using BonfireServer.Internal.Common;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Context.User;
using BonfireServer.Internal.Paths.Channel;
using BonfireServer.Internal.Paths.User;
using BonfireServer.Test;
using DotNetEnv;

namespace BonfireServer;

internal abstract class HttpServer
{
    private static readonly HttpListener Listener = new();
    private static readonly string BaseUrl = "http://localhost:8000/";
    private static readonly Dictionary<string, Tuple<byte[], long>> Files = new();
    private static readonly Regex PublicFileRegex = new(@"\/public\/(styles|scripts|pages)\/", RegexOptions.Compiled & RegexOptions.IgnoreCase);
    private static readonly JsonSerializerOptions JsonOptions = new(){ PropertyNameCaseInsensitive = true, AllowTrailingCommas = true };

    public static byte[]? GetFile(string name) => Files.GetValueOrDefault(name)?.Item1;
    public static long GetFileDate(string name) => Files.GetValueOrDefault(name)?.Item2 ?? 0;

    private static T? DeserializeBody<T>(ReqResMessage msg) where T : IBaseContext
    {
        try
        {
            if (msg.Request.HttpMethod == MethodTypes.Get)
            {
                var e = Activator.CreateInstance<T>();
                e.Token = msg.Request.Headers.Get("Authorization");
                return e;
            }
            
            var stream = new StreamReader(msg.Request.InputStream).ReadToEnd();
            if (stream.Length == 0 && msg.Request.HttpMethod != MethodTypes.Get)
            {
                var e = Activator.CreateInstance<T>();
                msg.IsValid = false;
                return e;
            }
            
            var ctx = JsonSerializer.Deserialize<T>(stream, JsonOptions);
            if (ctx != null)
                ctx.Token = msg.Request.Headers.Get("Authorization");
            else
                msg.IsValid = false;
            
            return ctx;
        }
        catch (ArgumentNullException ex)
        {
            Logger.Warn(ex.Message);
            msg.IsValid = false;
        }
        catch (JsonException ex)
        {
            Logger.Warn(ex.Message);
            msg.IsValid = false;
        }
        catch (NotSupportedException ex)
        {
            Logger.Warn(ex.Message);
            msg.IsValid = false;
        }
        return Activator.CreateInstance<T>();
    }
    
    private static ReqResMessage Router(ReqResMessage msg)
    {
        if (msg.Request.Url == null)
        {
            msg.Response.StatusCode = StatusCodes.BadRequest;
            return msg;
        }

        if (msg.Request.HttpMethod is MethodTypes.Options or MethodTypes.Head or MethodTypes.Trace or MethodTypes.Connect)
        {
            msg.Response.StatusCode = StatusCodes.MethodNotAllowed;
            return msg;
        }

        if (PublicFileRegex.IsMatch(msg.Request.RawUrl ?? ""))
        {
            var fileName = msg.Request.RawUrl!.Split("/").Last();
            var fileExtension = Path.GetExtension(fileName);
            var data = GetFile(fileName);
            var lastModified = GetFileDate(fileName);
            var ifModifiedSince = msg.Request.Headers.GetValues("If-Modified-Since")?[0] ?? "0";
            var contentType = fileExtension switch
            {
                ".js" => "text/javascript",
                ".css" => "text/css",
                ".html" => "text/html",
                _ => "text/plain"
            };

            if (data == null)
            {
                msg.Response.StatusCode = StatusCodes.NotFound;
                return msg;
            }
            if ((DateTime.FromBinary(long.Parse(ifModifiedSince)).Ticks >= lastModified))
            {
                msg.Response.StatusCode = StatusCodes.NotModified;
                return msg;
            }
            
            msg.Response.StatusCode = StatusCodes.Ok;
            msg.Response.ContentType = contentType;
            msg.Response.ContentEncoding = Encoding.UTF8;
            msg.Response.Headers["Last-Modified"] = lastModified.ToString("R");
            msg.Data = data;
            return msg;
        }

        switch (msg.Request.RawUrl ?? "")
        {
            ////////////////
            // HTML PAGES //
            ////////////////

            case "/":
                msg.Response.StatusCode = StatusCodes.Ok;
                msg.Response.ContentType = "text/html";
                msg.Response.ContentEncoding = Encoding.UTF8;
                msg.Data = Files["main.html"]?.Item1 ?? [];
                break;
            case "/favicon.ico":
                msg.Response.StatusCode = StatusCodes.NotImplemented;
                break;
            
            ///////////////////////////////////////
            // APPLICATION PROGRAMMING INTERFACE //
            ///////////////////////////////////////

            // Account Scope API
            case "/api/v1/account/get_self_user":
                var getSelfUserCtx = DeserializeBody<GetSelfUserContext>(msg);
                new GetSelfUserPath().Execute(msg, getSelfUserCtx);
                break;
            
            // Channel Scope API
            case "/api/v1/channel/get_messages":
                var getMessagesCtx = DeserializeBody<GetMessagesContext>(msg);
                new GetMessagesPath().Execute(msg, getMessagesCtx);
                break;
            case "/api/v1/channel/send_message":
                var sendMessageCtx = DeserializeBody<SendMessageContext>(msg);
                new SendMessagePath().Execute(msg, sendMessageCtx);
                break;
            case "/api/v1/channel/edit_message":
                var editMessageCtx = DeserializeBody<EditMessageContext>(msg);
                new EditMessagePath().Execute(msg, editMessageCtx);
                break;
            case "/api/v1/channel/delete_message":
                var deleteMessageCtx = DeserializeBody<DeleteMessageContext>(msg);
                new DeleteMessagePath().Execute<DeleteMessageContext>(msg, deleteMessageCtx);
                break;
            
            //////////////////
            // DEFAULT CASE //
            //////////////////
            default:
                msg.Response.StatusCode = StatusCodes.NotFound;
                break;
        }
        
        return msg;
    }
    
    private static async Task HandleIncomingConnections()
    {
        while (true)
        {
            var ctx = await Listener.GetContextAsync();
            
            try
            {
                // Will wait here until we hear from a connection
                var req = ctx.Request;
                var res = ctx.Response;
                
                if (ctx.Request.IsWebSocketRequest)
                {
                    ProcessRequest(ctx);
                }

                Logger.Info($"[{req.HttpMethod}] {req.Url?.ToString()} - {req.UserAgent}");
                
                var msg = Router(new ReqResMessage(req, res));
                await msg.Response.OutputStream.WriteAsync(msg.Data.AsMemory(0, msg.Data.Length));
                res.Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                
                ctx.Response.StatusCode = StatusCodes.InternalServerError;
                await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(string.Empty));
                ctx.Response.Close();
            }
        }
    }

    private static void LoadFrontendFiles()
    {
        var dirPath = Path.GetFullPath("./Public");
        
        if (!Path.Exists(dirPath))
             dirPath = Path.GetFullPath("../../../Public");

        foreach (var filePath in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(filePath) is ".ts" or ".tsx")
                continue;
            
            var filename = Path.GetFileName(filePath);
            Logger.Info($"Loading '{filename}' ...");
            Files[filename] = new (File.ReadAllBytes(filePath), File.GetLastWriteTime(filePath).Ticks);
        }
    }
    
    public static void Main(string[] args)
    {
        Listener.Prefixes.Add(BaseUrl);
        Listener.Start();
        Logger.Info($"Listening for connections on {BaseUrl}");
        
        // Generate a token so that we know a SECRET is set
        var _ = new AuthToken(new LiteFlakeId());
        Logger.Info(_.Val);
        
        // Start the Database so that the static constructor is called
        Database.Database.CreateIndexes();

        LoadFrontendFiles();

        var listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        Listener.Close();
    }
    
    private static int count = 0;
    private static async void ProcessRequest(HttpListenerContext listenerContext)
    {
        WebSocketContext webSocketContext;
        try
        { 
            webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
            Interlocked.Increment(ref count);
            Logger.Info($"Processed: {count}");
        }
        catch(Exception ex)
        { 
            listenerContext.Response.StatusCode = 500;
            listenerContext.Response.Close();
            Logger.Info($"Exception: {ex}");
            return;
        }
                                
        var webSocket = webSocketContext.WebSocket;                                           
        try
        {
            var receiveBuffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                switch (receiveResult.MessageType)
                {
                    case WebSocketMessageType.Close:
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    case WebSocketMessageType.Text:
                        Logger.Warn("Received text frame, rejecting.");
                        break;
                    default:
                        await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, true, CancellationToken.None);
                        break;
                }
            }
        }
        catch(Exception ex)
        { 
            Logger.Warn($"Exception: {ex}");
        }
        finally
        { 
            webSocket.Dispose();
        }
    }
}