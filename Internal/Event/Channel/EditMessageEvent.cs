using System.Net.WebSockets;

namespace BonfireServer.Internal.Event.Channel;

public class EditMessageEvent : BaseEvent
{
    protected override Dictionary<LiteFlakeId, WebSocket> Targets { get; } = GetTargets<EditMessageEvent>();
}