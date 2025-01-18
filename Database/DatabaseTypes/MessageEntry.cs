using BonfireServer.Internal.Common;

namespace BonfireServer.Database.DatabaseTypes;

[Serializable]
public class MessageEntry
{
    public long Id { get; set; }
    public long Channel { get; set; }
    public long Author { get; set; }
    
    public string? Content { get; set; } = null;
    public long LastEdited { get; set; }

    public MessageEntry(Message message)
    {
        Id = message.Id.Val;
        Channel = message.Channel.Id.Val;
        Author = message.Author.Id.Val;
        
        Content = message.Content;
        LastEdited = message.LastEdited;
    }
}