namespace BonfireServer.Internal.Common;

public class Message(LiteFlakeId? id)
{
    public LiteFlakeId Id { get; } = id ?? new LiteFlakeId();
    public Channel Channel { get; set; }
    public User Author { get; set; }
    
    public string? Content { get; set; } = null;
}