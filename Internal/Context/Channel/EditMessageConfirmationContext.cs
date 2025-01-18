using System.Text.Json.Serialization;

namespace BonfireServer.Internal.Context.Channel;

[Serializable]
public class EditMessageConfirmationContext : EditMessageContext
{
    public long LastEdited { get; set; }
}