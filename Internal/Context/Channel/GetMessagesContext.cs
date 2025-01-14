using System.Text.Json.Serialization;

namespace BonfireServer.Internal.Context.Channel;

[Serializable]
public class GetMessagesContext : IBaseContext
{
    [JsonIgnore] public string? Token { get; set; }
    public byte Count { get; set; } = 64;
    public long ChannelId { get; set; }
    public bool Greedy { get; set; }
    public long Before { get; set; } = long.MaxValue;
}