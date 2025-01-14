using System.Text;
using BonfireServer.Internal.Common;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Event.Channel;

namespace BonfireServer.Internal.Paths.Channel;

public class SendMessagePath : BasePath
{
    public override string Method { get; set; } = MethodTypes.Post;

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : default
    {
        if (!IsValid(msg, rawCtx) || rawCtx is not SendMessageContext ctx)
            return InvalidMessage(msg);
        if (!IsAuthorized(msg, rawCtx))
            return UnauthorizedTokenMessage(msg);

        var channel = Database.Database.FindChannel(ctx.ChannelId);
        var author = Database.Database.FindUserByToken(ctx.Token!);
        var content = ctx.Message;
        
        if (channel == null || author == null || content == null)
            return UnprocessableMessage(msg);

        Message.Create(content, channel, author);
        new SendMessageEvent().Emit(ctx);

        msg.Response.StatusCode = StatusCodes.Ok;
        msg.Response.ContentType = "application/json";
        msg.Response.ContentEncoding = Encoding.UTF8;
        return msg;
    }
}