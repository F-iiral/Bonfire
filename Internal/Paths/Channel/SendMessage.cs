using BonfireServer.Database.DatabaseTypes;
using BonfireServer.Internal.Common;
using BonfireServer.Internal.Context.Channel;

namespace BonfireServer.Internal.Paths.Channel;

public class SendMessagePath : BasePath
{
    public override string Method { get; set; } = "POST";

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : class
    {
        if (!IsValid(msg, rawCtx))
            return InvalidMessage(msg);
        if (rawCtx is not SendMessageContext ctx)
            return InvalidMessage(msg);

        var messageRaw = new Message(null);
        messageRaw.Channel = new Common.Channel(null);
        messageRaw.Author = new User(null);
        messageRaw.Content = ctx.Message;
        
        var message = new MessageEntry(messageRaw);
        
        Database.Database.SaveMessage(message);
        Logger.Info(ctx.Message);

        return msg;
    }
}