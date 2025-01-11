using System.Security.Cryptography;
using System.Text.Json.Serialization;
using BonfireServer.Database;

namespace BonfireServer.Internal.Common;

public class User(LiteFlakeId? id) : ICachableType
{
    public LiteFlakeId Id { get; set; } = id ?? new LiteFlakeId();
    public string Name { get; set; }
    public ushort Discriminator { get; set; }
    
    [JsonIgnore] public string Email { get; set; }
    [JsonIgnore] public byte[] PasswordHash { get; set; } = [];
    [JsonIgnore] public byte[] PasswordSalt { get; set; } = [];
    [JsonIgnore] public AuthToken AuthToken { get; set; }

    public string? Avatar { get; set; } = null;
    public string? Banner { get; set; } = null;
    public int Flags { get; set; } = 0;

    [JsonIgnore] public Dictionary<Server, string> Nicknames { get; set; } = new();
    [JsonIgnore] public List<Server> Servers { get; set; } = [];
    [JsonIgnore] public List<User> Friends { get; set; } = [];
    [JsonIgnore] public List<User> FriendRequests { get; set; } = [];
    [JsonIgnore] public List<Channel> DirectMessages { get; set; } = [];
    
    public static User RegisterNewUser(string name, string email, string password)
    {
        var user = new User(null);
        
        user.Name = name;
        user.Discriminator = FindNewDiscriminator(name);
        
        user.Email = email;
        user.PasswordSalt = RandomNumberGenerator.GetBytes(16);
        user.PasswordHash = Rfc2898DeriveBytes.Pbkdf2(password, user.PasswordSalt, 300000, HashAlgorithmName.SHA512, 512);
        user.AuthToken = new AuthToken(user.Id);

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