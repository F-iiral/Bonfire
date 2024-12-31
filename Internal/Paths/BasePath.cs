using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context;

namespace BonfireServer.Internal.Paths;

public abstract class BasePath
{
    public abstract string Method { get; set; }

    protected ReqResMessage InvalidMessage(ReqResMessage msg, bool invalidData=false)
    {
        if (invalidData)
            msg.Response.StatusCode = StatusCodes.UnprocessableContent;
        else if (msg.Request.HttpMethod != Method)
            msg.Response.StatusCode = StatusCodes.MethodNotAllowed;
        else if (msg.Request.Headers.Get("Authorization") == null)
            msg.Response.StatusCode = StatusCodes.Unauthorized;
        else
            msg.Response.StatusCode = StatusCodes.BadRequest;
        return msg;
    }
    
    protected bool IsValid<T>(ReqResMessage msg, T? rawCtx) where T : IBaseContext
    {
        return msg.Request.HttpMethod == Method && msg.IsValid && rawCtx != null && rawCtx.Token != null;
    }

    public abstract ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : IBaseContext;
}