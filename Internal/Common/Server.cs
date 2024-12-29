namespace BonfireServer.Internal.Common;

public class Server
{
    public LiteFlakeId Id { get; set; }
    
    public string Name { get; set; }
    
    public User Owner { get; set; }
    public List<Channel> Channels { get; set; }
    public List<Tuple<User, int>> Admins { get; set; }
    public List<User> Users { get; set; }
}