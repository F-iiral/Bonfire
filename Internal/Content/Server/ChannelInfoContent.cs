using BonfireServer.Internal.Common;

namespace BonfireServer.Internal.Content.Server;

public class ChannelInfoContent(Channel channel)
{
    public LiteFlakeId Id { get; set; } = channel.Id;

    public string Name { get; set; } = channel.Name;

    public Common.Server? Server { get; set; } = channel.Server;
    public List<Message> Messages { get; set; } = channel.Messages.Take(64).ToList();
}