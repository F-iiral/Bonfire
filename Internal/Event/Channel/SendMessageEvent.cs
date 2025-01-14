using System.Net.WebSockets;

namespace BonfireServer.Internal.Event.Channel;

public class SendMessageEvent : BaseEvent
{
    protected override Dictionary<LiteFlakeId, WebSocket> Targets { get; } = GetTargets<SendMessageEvent>();
}