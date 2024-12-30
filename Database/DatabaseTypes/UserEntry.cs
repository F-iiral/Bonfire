using BonfireServer.Internal;
using BonfireServer.Internal.Common;

namespace BonfireServer.Database.DatabaseTypes;

public class UserEntry
{
    public long Id { get; }
    public string Name { get; }
    public ushort Discriminator { get; }
    public string Email { get; }
    
    public byte[] PasswordHash { get; }
    public byte[] PasswordSalt { get; }

    public string? Avatar { get; } = null;
    public string? Banner { get; } = null;
    public int Flags { get; } = 0;
    
    public Dictionary<long, string> Nicknames { get; }
    public List<long> Servers { get; }
    public List<long> Friends { get; }
    public List<long> FriendRequests { get; }
    public List<long> DirectMessages { get; }

    public UserEntry(User user)
    {
        Id = user.Id.Val;
        Name = user.Name;
        Discriminator = user.Discriminator;
        Email = user.Email;
        PasswordHash = user.PasswordHash;
        PasswordSalt = user.PasswordSalt;
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