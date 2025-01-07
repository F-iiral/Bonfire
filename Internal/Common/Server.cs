using System.Text.Json.Serialization;
using BonfireServer.Database;
using BonfireServer.Internal.Const;

namespace BonfireServer.Internal.Common;

public class Server(LiteFlakeId? id) : ICachableType
{
    public LiteFlakeId Id { get; } = id ?? new LiteFlakeId();

    public string Name { get; set; }
    
    public User Owner { get; set; }
    [JsonIgnore] public List<Channel> Channels { get; set; } = [];
    [JsonIgnore] public List<Tuple<User, byte>> Admins { get; set; } = [];
    [JsonIgnore] public List<User> Users { get; set; } = [];

    public bool IsVerifiedMember(User user)
    {
        return Admins.Any(x => x.Item1 == user);
    }
    public bool IsModerator(User user)
    { 
        return Admins.Any(x => x.Item1 == user && x.Item2 >= AdminLevels.Moderator);
    }
    public bool IsStandardAdmin(User user)
    {
        return Admins.Any(x => x.Item1 == user && x.Item2 >= AdminLevels.StandardAdmin);
    }
    public bool IsPrivilegedAdmin(User user)
    {
        return Admins.Any(x => x.Item1 == user && x.Item2 >= AdminLevels.PrivilegedAdmin);
    } 
    public bool IsOwner(User user)
    {
        return Admins.Any(x => x.Item1 == user && x.Item2 >= AdminLevels.Owner) && user == Owner;
    }
}