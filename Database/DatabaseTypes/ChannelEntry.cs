using BonfireServer.Internal;
using BonfireServer.Internal.Common;

namespace BonfireServer.Database.DatabaseTypes;

public class ChannelEntry
{
    public long Id { get; }
    
    public string Name { get; }

    public long Server { get; }
    public List<long> Messages { get; }

    public ChannelEntry(Channel channel)
    {
        Id = channel.Id.Val;
        Name = channel.Name;
        Server = channel.Server?.Id.Val ?? 0;
        Messages = channel.Messages.Select(x => x.Id.Val).ToList();
    }
}