using BonfireServer.Internal.Const;

namespace BonfireServer.Internal.Paths;
using BonfireServer.Internal.Context;

public abstract class BasePath
{
    public abstract string Method { get; set; }

    protected ReqResMessage InvalidMessage(ReqResMessage msg)
    {
        if (msg.Request.HttpMethod != Method)
            msg.Response.StatusCode = StatusCodes.MethodNotAllowed;
        else
            msg.Response.StatusCode = StatusCodes.BadRequest;
        return msg;
    }
    
    protected bool IsValid<T>(ReqResMessage msg, T? rawCtx) where T : BaseContext
    {
        return msg.Request.HttpMethod == Method && msg.IsValid && rawCtx != null;
    }

    public abstract ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : BaseContext;
}