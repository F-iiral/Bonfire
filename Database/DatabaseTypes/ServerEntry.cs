using BonfireServer.Internal;
using BonfireServer.Internal.Common;

namespace BonfireServer.Database.DatabaseTypes;

public class ServerEntry
{
    public long Id { get; }
    
    public string Name { get; }
    
    public long Owner { get; }
    public List<long> Channels { get; }
    public List<Tuple<long, byte>> Admins { get; }
    public List<long> Users { get; }

    public ServerEntry(Server server)
    {
        Id = server.Id.Val;
        Name = server.Name;
        Owner = server.Owner.Id.Val;
        Channels = server.Channels.Select(x => x.Id.Val).ToList();
        Admins = server.Admins.Select(x => new Tuple<long, byte>(x.Item1.Id.Val, x.Item2)).ToList();
        Users = server.Users.Select(x => x.Id.Val).ToList();
    }
}