using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BonfireServer.Internal;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context;
using BonfireServer.Internal.Context.Account;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Event;
using BonfireServer.Internal.Event.Channel;
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
        
        Database.Database.CreateIndexes();
        HttpServer.LoadFrontendFiles();

        var testToken = new AuthToken(new LiteFlakeId());
        var pingTask = Task.Run(WebsocketServer.LoopAsync); 
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
    private const int TimeoutMilliseconds = 10000;
    private static readonly ArraySegment<byte> PingData = new(Encoding.UTF8.GetBytes("ACK"), 0, "ACK".Length);
    public static readonly Dictionary<LiteFlakeId, WebSocket> Targets = new();

    public static void CleanSocket(LiteFlakeId id, WebSocket socket, string reason)
    {
        socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
        
        BaseEvent.RemoveTargetFromAll(id);
        Targets.Remove(id);
        socket.Dispose();
    }
    
    public static void SendData(LiteFlakeId id, WebSocket socket, ArraySegment<byte> data)
    {
        try
        {
            if (socket.State != WebSocketState.Open)
                CleanSocket(id, socket, "Client Closed");

            socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Exception while sending data over websocket: {ex}");
            CleanSocket(id, socket, "Exception occured");
        }
    }
    
    private static Task RegisterNewSocket(WebSocket socket)
    {
        var buffer = new byte[16384];
        var receiveTask = Task.Run(() => socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None));
        var idleTask = Task.Delay(TimeoutMilliseconds);
        
        var result = Task.WhenAny(receiveTask, idleTask);
        result.Wait();
        
        if (result.Result == idleTask)
        {
            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Timeout", CancellationToken.None);
            socket.Dispose();
            return Task.CompletedTask;
        }

        try
        {
            var rawToken = Encoding.UTF8.GetString(buffer.Take(buffer.Length - 1).ToArray());
            if (rawToken == null || rawToken == "")
            {
                socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid Token", CancellationToken.None);
                socket.Dispose();
                return Task.CompletedTask;
            }

            var splitToken = rawToken.Split('.');
            if (splitToken.Length != 4)
            {
                socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid Token", CancellationToken.None);
                socket.Dispose();
                return Task.CompletedTask;
            }

            var success = long.TryParse(Encoding.UTF8.GetString(Convert.FromBase64String(splitToken[0])),
                out var userIdLong);
            if (!success)
            {
                socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid Token", CancellationToken.None);
                socket.Dispose();
                return Task.CompletedTask;
            }

            var userId = new LiteFlakeId(userIdLong);
            Targets[userId] = socket;
            Logger.Info($"User {userId} connected");

            // Temp
            BaseEvent.AddTarget(typeof(DeleteMessageEvent), userId, socket);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Exception while receiving token over websocket: {ex}");
        }
        
        return Task.CompletedTask;
    }
    
    private static Task CheckUser(LiteFlakeId id, WebSocket socket)
    {
        SendData(id, socket, PingData);
        
        var buffer = new byte[16384];
        var receiveTask = Task.Run(() => socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None));
        var idleTask = Task.Delay(TimeoutMilliseconds);
        
        var result = Task.WhenAny(receiveTask, idleTask);
        result.Wait();
        
        if (result.Result == idleTask)
            CleanSocket(id, socket, "Timeout");
        
        return Task.CompletedTask;
    }

    public static async Task LoopAsync()
    {
        while (true)
        {
            await Task.WhenAll(Targets.Select(target => CheckUser(target.Key, target.Value)));
            await Task.Delay(TimeoutMilliseconds);
        }
    }

    public static async void ProcessRequest(HttpListenerContext listenerContext)
    {
        WebSocketContext webSocketContext;
        try
        { 
            webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
            Interlocked.Increment(ref Count);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Exception while accepting websocket: {ex}");
            listenerContext.Response.StatusCode = StatusCodes.InternalServerError;
            listenerContext.Response.Close();
            return;
        }
        
        var webSocket = webSocketContext.WebSocket;
        await Task.Run(() => RegisterNewSocket(webSocket));
    }
}