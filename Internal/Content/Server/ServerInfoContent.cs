using BonfireServer.Internal.Common;

namespace BonfireServer.Internal.Content.Server;

public class ServerInfoContent(Common.Server server) : IBaseContent
{    public LiteFlakeId Id { get; set; } = server.Id;

    public string Name { get; set; } = server.Name;

    public User Owner { get; set; } = server.Owner;
    public List<Channel> Channels { get; set; } = server.Channels;
    public List<Tuple<User, byte>> Admins { get; set; } = server.Admins;
    public List<User> Users { get; set; } = server.Users;
}