using BonfireServer.Internal.Common;
using BonfireServer.Internal.Content.Server;

namespace BonfireServer.Internal.Content.Account;

public class SelfUser(User user) : IBaseContent
{
    public User User { get; set; } = user;
    public string Email { get; set; } = user.Email;
    public Dictionary<ServerInfo, string> Nicknames { get; set; } = user.Nicknames.ToDictionary(x => new ServerInfo(x.Key), x => x.Value);
    public List<ServerInfo> Servers { get; set; } = user.Servers.Select(x => new ServerInfo(x)).ToList();
    public List<User> Friends { get; set; } = user.Friends;
    public List<User> FriendRequests { get; set; } = user.FriendRequests;
    public List<Channel> DirectMessages { get; set; } = user.DirectMessages;
}