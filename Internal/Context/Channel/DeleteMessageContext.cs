namespace BonfireServer.Internal.Context.Channel;

[Serializable]
public class DeleteMessageContext : IBaseContext
{
    public string? Token { get; set; }
    public long ChannelId { get; set; }
    public long MessageId { get; set; }
}