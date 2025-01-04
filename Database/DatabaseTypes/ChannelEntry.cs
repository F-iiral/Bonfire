using BonfireServer.Internal;
using BonfireServer.Internal.Common;

namespace BonfireServer.Database.DatabaseTypes;

[Serializable]
public class ChannelEntry
{
    public long Id { get; set; }
    
    public string Name { get; set; }

    public long Server { get; set; }
    public List<long> Messages { get; set; }

    public ChannelEntry(Channel channel)
    {
        Id = channel.Id.Val;
        Name = channel.Name;
        Server = channel.Server?.Id.Val ?? 0;
        Messages = channel.Messages.Select(x => x.Id.Val).ToList();
    }
}