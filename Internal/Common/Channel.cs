namespace BonfireServer.Internal.Common;

public class Channel
{
    public LiteFlakeId Id { get; set; }
    public Server Server { get; set; }
    
    public string Name { get; set; }
    
    public List<Message> Messages { get; set; }
}