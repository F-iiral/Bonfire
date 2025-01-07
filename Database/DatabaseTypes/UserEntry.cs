using BonfireServer.Internal;
using BonfireServer.Internal.Common;

namespace BonfireServer.Database.DatabaseTypes;

[Serializable]
public class UserEntry
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ushort Discriminator { get; set; }
    public string Email { get; set; }
    
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string AuthToken { get; set; }
    
    public string? Avatar { get; set; } = null;
    public string? Banner { get; set; } = null;
    public int Flags { get; set; } = 0;
    
    public Dictionary<long, string> Nicknames { get; set; }
    public List<long> Servers { get; set; }
    public List<long> Friends { get; set; }
    public List<long> FriendRequests { get; set; }
    public List<long> DirectMessages { get; set; }

    public UserEntry(User user)
    {
        Id = user.Id.Val;
        Name = user.Name;
        Discriminator = user.Discriminator;
        Email = user.Email;
        PasswordHash = user.PasswordHash;
        PasswordSalt = user.PasswordSalt;
        AuthToken = user.AuthToken.Val;
        Avatar = user.Avatar;
        Banner = user.Banner;
        Flags = user.Flags;
        Nicknames = user.Nicknames.ToDictionary(x => x.Key.Id.Val, x => x.Value);
        Servers = user.Servers.Select(x => x.Id.Val).ToList();
        Friends = user.Friends.Select(x => x.Id.Val).ToList();
        FriendRequests = user.FriendRequests.Select(x => x.Id.Val).ToList();
        DirectMessages = user.DirectMessages.Select(x => x.Id.Val).ToList();
    }
}