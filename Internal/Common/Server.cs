using BonfireServer.Database;
using BonfireServer.Internal.Const;

namespace BonfireServer.Internal.Common;

public class Server(LiteFlakeId? id) : ICachableType
{
    public LiteFlakeId Id { get; } = id ?? new LiteFlakeId();

    public string Name { get; set; }
    
    public User Owner { get; set; }
    public List<Channel> Channels { get; set; } = [];
    public List<Tuple<User, byte>> Admins { get; set; } = [];
    public List<User> Users { get; set; } = [];
}