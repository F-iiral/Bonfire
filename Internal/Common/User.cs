namespace BonfireServer.Internal.Common;

public class User
{
    public LiteFlakeId Id { get; set; }
    
    public string Name { get; set; }
    public ushort Discriminator {get; set;}
    
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    
    public string Avatar { get; set; }
    public string Banner { get; set; }
    public int Flags { get; set; }

    public Dictionary<Server, string> Nicknames { get; set; }
    public List<Server> Servers { get; set; }
    public List<User> Friends { get; set; }
    public List<User> FriendRequests { get; set; }
    public List<Channel> DirectMessages { get; set; }
}