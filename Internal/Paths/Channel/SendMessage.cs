namespace BonfireServer.Internal.Paths.Channel;
using BonfireServer.Internal.Context.Channel;

public class SendMessagePath : BasePath
{
    public override string Method { get; set; } = "POST";

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : class
    {
        if (!IsValid(msg, rawCtx))
            return InvalidMessage(msg);
        if (rawCtx is not SendMessageContext ctx)
            return InvalidMessage(msg);
        
        Logger.Debug(ctx.Message);

        return msg;
    }
}