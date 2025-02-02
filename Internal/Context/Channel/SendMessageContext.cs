using System.Text.Json.Serialization;

namespace BonfireServer.Internal.Context.Channel;

[Serializable]
public class SendMessageContext : IBaseContext
{
    [JsonIgnore] public string? Token { get; set; }
    public string? Message { get; set; }
    public long ChannelId { get; set; }
}