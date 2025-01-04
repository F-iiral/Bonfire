namespace BonfireServer.Internal.Context.Channel;

public class GetMessagesContext : IBaseContext
{
    public string? Token { get; set; }
    public byte Count { get; set; } = 64;
    public long ChannelId { get; set; }
}