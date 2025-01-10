using BonfireServer.Internal.Common;

namespace BonfireServer.Internal.Content.Account;

public class SelfUser(User user) : IBaseContent
{
    public User User { get; set; } = user;
    public string Email { get; set; } = user.Email;
    public Dictionary<Server, string> Nicknames { get; set; } = user.Nicknames;
    public List<Server> Servers { get; set; } = user.Servers;
    public List<User> Friends { get; set; } = user.Friends;
    public List<User> FriendRequests { get; set; } = user.FriendRequests;
    public List<Channel> DirectMessages { get; set; } = user.DirectMessages;
}