using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BonfireServer.Internal.Common;
using BonfireServer.Internal.Const;
using BonfireServer.Internal.Context.Channel;
using BonfireServer.Internal.Converters;
using MongoDB.Bson;

namespace BonfireServer.Internal.Paths.Channel;

public class GetMessagesPath : BasePath
{
    public override string Method { get; set; } = MethodTypes.Query;

    public override ReqResMessage Execute<T>(ReqResMessage msg, T? rawCtx) where T : default
    {
        if (!IsValid(msg, rawCtx) || rawCtx is not GetMessagesContext ctx)
            return InvalidMessage(msg);
        if (!IsAuthorized(msg, rawCtx))
            return UnauthorizedTokenMessage(msg);

        var channel = Database.Database.FindChannel(ctx.ChannelId);

        if (channel == null)
            return UnprocessableMessage(msg);
        if (ctx.Greedy)
            Database.Database.TryExtendChannelMessages(channel, out _, ctx.Before);

        var lastMessages = channel.Messages.Where(m => m.Id < ctx.Before).Take(ctx.Count).ToList();

        msg.Response.StatusCode = StatusCodes.Ok;
        msg.Response.ContentType = "application/json";
        msg.Response.ContentEncoding = Encoding.UTF8;
        msg.Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(lastMessages, JsonOptions));
        return msg;
    }
}