namespace BonfireServer.Internal.Common;

public class Message
{
    public LiteFlakeId Id { get; set; }
    public Channel Channel { get; set; }
    public User Author { get; set; }
    
    public string Content { get; set; }
}