namespace BonfireServer.Internal.Context.Channel;

public class SendMessageContext : IBaseContext
{
    public string? Token { get; set; }
    public string? Message { get; set; }
    public long ChannelId { get; set; }
    public long AuthorId { get; set; }
}