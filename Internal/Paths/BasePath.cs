using System.Text.Json;
using System.Text.Json.Serialization;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context;
using BonfireServer.Internal.Converters;

namespace BonfireServer.Internal.Paths;

public abstract class BasePath
{
    public abstract string Method { get; set; }
    public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        Converters = { new LiteFlakeIdJsonConverter() },
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyProperties = true,
        IgnoreReadOnlyFields = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    protected ReqResMessage UnauthorizedTokenMessage(ReqResMessage msg)
    {
        msg.Response.StatusCode = StatusCodes.Unauthorized;
        return msg;
    }
    protected ReqResMessage InsufficientPermissionMessage(ReqResMessage msg) {
        msg.Response.StatusCode = StatusCodes.Forbidden;
        return msg;
    }
    protected ReqResMessage UnprocessableMessage(ReqResMessage msg)
    {
        msg.Response.StatusCode = StatusCodes.UnprocessableContent;
        return msg; 
    }
    protected ReqResMessage InvalidMessage(ReqResMessage msg)
    {
        if (msg.Request.HttpMethod != Method)
            msg.Response.StatusCode = StatusCodes.MethodNotAllowed;
        else if (msg.Request.Headers.Get("Authorization") == null)
            msg.Response.StatusCode = StatusCodes.Unauthorized;
        else
            msg.Response.StatusCode = StatusCodes.BadRequest;
        return msg;
    }
    
    protected bool IsValid<T>(ReqResMessage msg, T? rawCtx) where T : IBaseContext
    {
        return msg.Request.HttpMethod == Method && msg.IsValid && rawCtx != null;
    }

    protected bool IsAuthorized<T>(ReqResMessage msg, T? rawCtx) where T : IBaseContext
    {
        if (rawCtx == null || rawCtx.Token == null)
            return false;

        var splitToken = rawCtx.Token.Split('.');
        if (splitToken.Length != 4)
            return false;
        
//        var timestamp = (Encoding.UTF8.GetString(Convert.FromBase64String(splitToken[1])));
//        Logger.Info(timestamp.ToString());
//        if (timestamp > DateTime.Now.ToUniversalTime().Ticks - 28 * 24 * 60 * 60)
//            return false;
        
        var user = Database.Database.FindUserByToken(rawCtx.Token);
        if (user == null)
            return false;
        
        return user.AuthToken == rawCtx.Token;
    }

    public abstract ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : IBaseContext;
}