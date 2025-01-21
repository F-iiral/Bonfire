using System.Text.Json.Serialization;

namespace BonfireServer.Internal.Context.Channel;

[Serializable]
public class SendMessageConfirmationContext : SendMessageContext
{
    public long MessageId { get; set; }
    public long AuthorId { get; set; }
}