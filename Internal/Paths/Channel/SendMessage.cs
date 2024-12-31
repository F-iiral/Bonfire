using BonfireServer.Database.DatabaseTypes;
using BonfireServer.Internal.Common;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context.Channel;

namespace BonfireServer.Internal.Paths.Channel;

public class SendMessagePath : BasePath
{
    public override string Method { get; set; } = MethodTypes.Post;

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : default
    {
        if (!IsValid(msg, rawCtx))
            return InvalidMessage(msg);
        if (rawCtx is not SendMessageContext ctx)
            return InvalidMessage(msg);

        var message = new Message(null);
        message.Channel = new Common.Channel(null);
        message.Author = new User(null);
        message.Content = ctx.Message;
        
        Database.Database.SaveMessage(message);
        Logger.Info(ctx.Message);

        return msg;
    }
}