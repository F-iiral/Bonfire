using System.Text.Json.Serialization;

namespace BonfireServer.Internal.Context.Channel;

[Serializable]
public class DeleteMessageContext : IBaseContext
{
    [JsonIgnore] public string? Token { get; set; }
    public long ChannelId { get; set; }
    public long MessageId { get; set; }
}