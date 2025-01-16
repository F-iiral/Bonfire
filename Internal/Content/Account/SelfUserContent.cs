using BonfireServer.Internal.Common;
using BonfireServer.Internal.Content.Server;

namespace BonfireServer.Internal.Content.Account;

public class SelfUserContent(User user) : IBaseContent
{
    public User User { get; set; } = user;
    public string Email { get; set; } = user.Email;
    public Dictionary<ServerInfoContent, string> Nicknames { get; set; } = user.Nicknames.ToDictionary(x => new ServerInfoContent(x.Key), x => x.Value);
    public List<ServerInfoContent> Servers { get; set; } = user.Servers.Select(x => new ServerInfoContent(x)).ToList();
    public List<User> Friends { get; set; } = user.Friends;
    public List<User> FriendRequests { get; set; } = user.FriendRequests;
    public List<ChannelInfoContent> DirectMessages { get; set; } = user.DirectMessages.Select(x => new ChannelInfoContent(x)).ToList();
    public List<ChannelInfoContent> Channels { get; set; } = user.Servers
        .SelectMany(server => server.Channels.Select(channel => new ChannelInfoContent(channel)))
        .ToList();
}