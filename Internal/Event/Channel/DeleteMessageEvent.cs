using System.Net.WebSockets;

namespace BonfireServer.Internal.Event.Channel;

public class DeleteMessageEvent : BaseEvent
{
    protected override Dictionary<LiteFlakeId, WebSocket> Targets { get; } = GetTargets<DeleteMessageEvent>();
}