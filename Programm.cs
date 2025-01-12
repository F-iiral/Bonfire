using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BonfireServer.Internal;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Context.User;
using BonfireServer.Internal.Paths.Channel;
using BonfireServer.Internal.Paths.User;

namespace BonfireServer;

internal abstract class Program
{
    public static void Main(string[] args)
    {
        HttpServer.Listener.Prefixes.Add(HttpServer.BaseUrl);
        HttpServer.Listener.Start();
        Logger.Info($"Listening for connections on {HttpServer.BaseUrl}");
        
        // Generate a token so that we know a SECRET is set
        var _ = new AuthToken(new LiteFlakeId());
        Logger.Info(_.Val);
        
        // Start the Database so that the static constructor is called
        Database.Database.CreateIndexes();

        HttpServer.LoadFrontendFiles();

        var listenTask = HttpServer.HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        HttpServer.Listener.Close();
    }
}

internal static class HttpServer
{
    public static readonly HttpListener Listener = new();
    public static readonly string BaseUrl = "http://localhost:8000/";
    public static readonly Dictionary<string, Tuple<byte[], long>> Files = new();
    public static readonly Regex PublicFileRegex = new(@"\/public\/(styles|scripts|pages)\/", RegexOptions.Compiled & RegexOptions.IgnoreCase);
    public static readonly JsonSerializerOptions JsonOptions = new(){ PropertyNameCaseInsensitive = true, AllowTrailingCommas = true };

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
                msg.Data = Files["main.html"].Item1 ?? [];
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
                new DeleteMessagePath().Execute(msg, deleteMessageCtx);
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
    
    public static async Task HandleIncomingConnections()
    {
        while (true)
        {
            var ctx = await Listener.GetContextAsync();
            
            try
            {
                // Will wait here until we hear from a connection
                var req = ctx.Request;
                var res = ctx.Response;
                
                Logger.Info($"[{req.HttpMethod}] {req.Url?.ToString()} - {req.UserAgent}");
                if (ctx.Request.IsWebSocketRequest)
                {
                    WebsocketServer.ProcessRequest(ctx);
                    continue;
                }

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

    public static void LoadFrontendFiles()
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
    
}

internal static class WebsocketServer
{
    private static int Count;
    
    public static async void ProcessRequest(HttpListenerContext listenerContext)
    {
        WebSocketContext webSocketContext;
        try
        { 
            webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
            Interlocked.Increment(ref Count);
            Logger.Info($"Processed: {Count}");
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
            while (webSocket.State == WebSocketState.Open)
            {
                var receiveBuffer = new byte[16384];
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                switch (receiveResult.MessageType)
                {
                    case WebSocketMessageType.Close:
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    case WebSocketMessageType.Text:
                        var receivedText = Encoding.UTF8.GetString(receiveBuffer, 0, receiveBuffer.Length);
                        Logger.Info($"Received: {receivedText}");
                        await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Text, true, CancellationToken.None);
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