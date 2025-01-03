using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BonfireServer.Internal;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Paths.Channel;

namespace BonfireServer;

internal abstract class HttpServer
{
    private static readonly HttpListener Listener = new();
    private static readonly string BaseUrl = "http://localhost:8000/";
    private static readonly Dictionary<string, byte[]> Files = new();
    private static readonly Regex PublicFileRegex = new(@"\/public\/(styles|scripts|pages)\/", RegexOptions.Compiled & RegexOptions.IgnoreCase);

    public static byte[]? GetFile(string name) => Files.GetValueOrDefault(name);

    private static T? DeserializeBody<T>(ReqResMessage msg) where T : IBaseContext
    {
        try
        {
            var opt = new JsonSerializerOptions();
            opt.PropertyNameCaseInsensitive = true;
            opt.AllowTrailingCommas = true;

            var stream = new StreamReader(msg.Request.InputStream).ReadToEnd();

            if (stream.Length == 0 && msg.Request.HttpMethod != MethodTypes.Get)
            {
                var e = Activator.CreateInstance<T>();
                msg.IsValid = false;
                return e;
            }
            
            var ctx = JsonSerializer.Deserialize<T>(stream, opt);
            
            if (ctx != null)
                ctx.Token = msg.Request.Headers.Get("Authorization");
            else
                msg.IsValid = false;
            
            return ctx;
        }
        catch (ArgumentNullException e)
        {
            Logger.Warn(e.Message);
            msg.IsValid = false;
        }
        catch (JsonException e)
        {
            Logger.Warn(e.Message);
            msg.IsValid = false;
        }
        catch (NotSupportedException e)
        {
            Logger.Warn(e.Message);
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

        if (msg.Request.HttpMethod is MethodTypes.Options or MethodTypes.Head or MethodTypes.Trace)
        {
            msg.Response.StatusCode = StatusCodes.MethodNotAllowed;
            return msg;
        }

        if (PublicFileRegex.IsMatch(msg.Request.RawUrl ?? ""))
        {
            var fileName = msg.Request.RawUrl!.Split("/").Last();
            var fileExtension = Path.GetExtension(fileName);
            var contentType = fileExtension switch
            {
                ".js" => "text/javascript",
                ".css" => "text/css",
                ".html" => "text/html",
                _ => "text/plain"
            };

            var data = GetFile(fileName);

            if (data == null)
            {
                msg.Response.StatusCode = StatusCodes.NotFound;
                return msg;
            }
            
            msg.Response.StatusCode = StatusCodes.Ok;
            msg.Response.ContentType = contentType;
            msg.Response.ContentEncoding = Encoding.UTF8;
            msg.Data = data;
            return msg;
        }
        
        switch (msg.Request.RawUrl ?? "")
        {
            case "/":
                msg.Response.StatusCode = StatusCodes.Ok;
                msg.Response.ContentType = "text/html";
                msg.Response.ContentEncoding = Encoding.UTF8;
                msg.Data = Files["main.html"];
                break;
            case "/grr":
                msg.Response.StatusCode = StatusCodes.Gone;
                break;
            case "/favicon.ico":
                msg.Response.StatusCode = StatusCodes.NotImplemented;
                break;
            case "/api/v1/channel/send_message":
                var ctx = DeserializeBody<SendMessageContext>(msg);
                new SendMessagePath().Execute(msg, ctx);
                break;
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
            if (Path.GetExtension(filePath) == ".ts" )
                continue;
            
            var filename = Path.GetFileName(filePath);
            Logger.Info($"Loading '{filename}' ...");
            Files[filename] = File.ReadAllBytes(filePath);
        }
    }
    
    public static void Main(string[] args)
    {
        Listener.Prefixes.Add(BaseUrl);
        Listener.Start();
        Logger.Info($"Listening for connections on {BaseUrl}");
        
        // Start the Database so that the static constructor is called
        Database.Database.CreateIndexes();

        LoadFrontendFiles();

        var listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        Listener.Close();
    }
}