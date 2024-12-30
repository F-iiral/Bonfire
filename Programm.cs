using System.Net;
using System.Text;
using System.Text.Json;
using BonfireServer.Database;
using BonfireServer.Internal;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Paths.Channel;

namespace BonfireServer;

internal abstract class HttpServer
{
    private static HttpListener Listener;
    private static readonly string BaseUrl = "http://localhost:8000/";
    
    private static string pageData =
        "<!DOCTYPE>" +
        "<html>" +
        "  <head>" +
        "    <title>HttpListener Example</title>" +
        "  </head>" +
        "  <body>" +
        "    <p>:3</p>" +
        "</html>";

    private static T? DeserializeBody<T>(ReqResMessage msg) where T : BaseContext
    {
        try
        {
            var opt = new JsonSerializerOptions();
            opt.PropertyNameCaseInsensitive = true;
            opt.AllowTrailingCommas = true;

            if (msg.Request.InputStream.Length == 0 && msg.Request.HttpMethod != MethodTypes.Get)
            {
                var e = Activator.CreateInstance<T>();
                msg.IsValid = false;
                return e;
            }
            
            var ctx = JsonSerializer.Deserialize<T>(msg.Request.InputStream, opt);
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
        
        switch (msg.Request.Url.ToString().Replace(BaseUrl, string.Empty))
        {
            case "":
                msg.Response.StatusCode = StatusCodes.Ok;
                msg.Response.ContentType = "text/html";
                msg.Response.ContentEncoding = Encoding.UTF8;
                msg.Data = Encoding.UTF8.GetBytes(pageData);
                break;
            case "grr":
                msg.Response.StatusCode = StatusCodes.Gone;
                break;
            case "api/v1/channel/send_message":
                var ctx = DeserializeBody<SendMessageContext>(msg);
                new SendMessagePath().Execute(msg, ctx);
                break;
            default:
                msg.Response.StatusCode = StatusCodes.BadRequest;
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
    
    public static void Main(string[] args)
    {
        Listener = new HttpListener();
        Listener.Prefixes.Add(BaseUrl);
        Listener.Start();
        Logger.Info($"Listening for connections on {BaseUrl}");
        
        // Start the Database so that the static constructor is called
        Database.Database.CreateIndexes();

        var listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        Listener.Close();
    }
}