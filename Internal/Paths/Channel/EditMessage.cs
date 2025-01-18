using System.Text;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Event.Channel;

namespace BonfireServer.Internal.Paths.Channel;

public class EditMessagePath : BasePath
{
    public override string Method { get; set; } = MethodTypes.Patch;

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : default
    {
        if (!IsValid(msg, rawCtx) || rawCtx is not EditMessageContext ctx)
            return InvalidMessage(msg);
        if (!IsAuthorized(msg, rawCtx))
            return UnauthorizedTokenMessage(msg);

        var channel = Database.Database.FindChannel(ctx.ChannelId);
        var user = Database.Database.FindUserByToken(ctx.Token!);
        var message = Database.Database.FindMessage(ctx.MessageId);
        var newContent = ctx.Message;

        if (channel == null || user == null || newContent == null || message == null)
            return UnprocessableMessage(msg);
        if (user != message.Author && !(channel.Server != null && channel.Server.Users.Contains(user)))
            return InsufficientPermissionMessage(msg);

        message.Edit(newContent);
        var confirmationCtx = (EditMessageConfirmationContext)ctx ;
        confirmationCtx.LastEdited = DateTime.Now.Ticks;
        new EditMessageEvent().Emit(confirmationCtx);

        msg.Response.StatusCode = StatusCodes.Ok;
        msg.Response.ContentType = "application/json";
        msg.Response.ContentEncoding = Encoding.UTF8;
        return msg;
    }
}