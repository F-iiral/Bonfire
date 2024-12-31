using System.Security.Cryptography;
using System.Text;
using BonfireServer.Database;

namespace BonfireServer.Internal.Common;

public class User(LiteFlakeId? id) : ICachableType
{
    public LiteFlakeId Id { get; protected set; } = id ?? new LiteFlakeId();
    public string Name { get; set; }
    public ushort Discriminator { get; set; }
    public string Email { get; set; }
    
    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];

    public string? Avatar { get; set; } = null;
    public string? Banner { get; set; } = null;
    public int Flags { get; set; } = 0;

    public Dictionary<Server, string> Nicknames { get; set; } = new();
    public List<Server> Servers { get; set; } = [];
    public List<User> Friends { get; set; } = [];
    public List<User> FriendRequests { get; set; } = [];
    public List<Channel> DirectMessages { get; set; } = [];
    
    public static User RegisterNewUser(string name, string email, string password)
    {
        var user = new User(null);
        
        user.Name = name;
        user.Discriminator = FindNewDiscriminator(name);
        
        user.Email = email;
        user.PasswordSalt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        user.PasswordHash = Rfc2898DeriveBytes.Pbkdf2(password, user.PasswordSalt, 300000, HashAlgorithmName.SHA512, 512);

        user.Avatar = "";
        user.Banner = "";
        user.Flags = 0;
        
        user.Nicknames = new();
        user.Servers = [];
        user.Friends = [];
        user.FriendRequests = [];
        user.DirectMessages = [];

        return user;
    }

    protected static ushort FindNewDiscriminator(string name)
    {
        // ToDo: Add this function :3
        return 0;
    }
}