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

        var channel = Database.Database.FindChannel(ctx.ChannelId);
        var author = Database.Database.FindUser(ctx.AuthorId);
        var content = ctx.Message;
        
        if (channel == null || author == null || content == null)
            return InvalidMessage(msg, true);

        var message = new Message(null);
        message.Channel = channel;
        message.Author = author;
        message.Content = content;
        
        Database.Database.SaveMessage(message);

        return msg;
    }
}