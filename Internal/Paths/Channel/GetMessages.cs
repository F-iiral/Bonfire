using System.Text;
using BonfireServer.Internal.Common;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context.Channel;
using MongoDB.Bson;

namespace BonfireServer.Internal.Paths.Channel;

public class GetMessagesPath : BasePath
{
    public override string Method { get; set; } = MethodTypes.Query;

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : default
    {
        if (!IsValid(msg, rawCtx))
            return InvalidMessage(msg);
        if (!IsAuthorized(msg, rawCtx))
            return InvalidMessage(msg);
        if (rawCtx is not GetMessagesContext ctx)
            return InvalidMessage(msg);

        var channel = Database.Database.FindChannel(ctx.ChannelId);
        
        if (channel == null)
            return UnprocessableMessage(msg);

        if (ctx.Greedy)
            Database.Database.TryExtendChannelMessages(channel, out _);
        
        var messagesCount = channel.Messages.Count;
        var lastMessages = channel.Messages.Skip(Math.Max(0, messagesCount - ctx.Count));

        msg.Response.StatusCode = StatusCodes.Ok;
        msg.Response.ContentType = "application/json";
        msg.Response.ContentEncoding = Encoding.UTF8;
        msg.Data = Encoding.UTF8.GetBytes(lastMessages.ToJson());
        return msg;
    }
}