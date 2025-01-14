using System.Text;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Event.Channel;

namespace BonfireServer.Internal.Paths.Channel;

public class DeleteMessagePath : BasePath
{
    public override string Method { get; set; } = MethodTypes.Delete;

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : default
    {
        if (!IsValid(msg, rawCtx) || rawCtx is not DeleteMessageContext ctx)
            return InvalidMessage(msg);
        if (!IsAuthorized(msg, rawCtx))
            return UnauthorizedTokenMessage(msg);

        var channel = Database.Database.FindChannel(ctx.ChannelId);
        var user = Database.Database.FindUserByToken(ctx.Token!);
        var message = Database.Database.FindMessage(ctx.MessageId);

        if (channel == null || user == null || message == null)
            return UnprocessableMessage(msg);
        if (user != message.Author && !(channel.Server != null && channel.Server.IsModerator(user)))
            return InsufficientPermissionMessage(msg);

        message.Delete();
        new DeleteMessageEvent().Emit(ctx);

        msg.Response.StatusCode = StatusCodes.Ok;
        msg.Response.ContentType = "application/json";
        msg.Response.ContentEncoding = Encoding.UTF8;
        return msg;
    }
}