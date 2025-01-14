using BonfireServer.Internal.Common;

namespace BonfireServer.Database.DatabaseTypes;

[Serializable]
public class ServerEntry
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    
    public long Owner { get; set; }
    public List<long> Channels { get; set; }
    public List<Tuple<long, byte>> Admins { get; set; }
    public List<long> Users { get; set; }

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