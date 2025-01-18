using System.Web;
using BonfireServer.Database;

namespace BonfireServer.Internal.Common;

public class Message(LiteFlakeId? id) : ICachableType
{
    public LiteFlakeId Id { get; set; } = id ?? new LiteFlakeId();
    public Channel Channel { get; set; }
    public User Author { get; set; }
    
    public string? Content { get; set; } = null;
    public long LastEdited { get; set; } = -1;

    public void Delete()
    {
        Channel.Messages.Remove(this);
        Database.Database.DeleteMessage(this);
    }
    public void Edit(string newContent)
    {
        Content = HttpUtility.HtmlEncode(newContent);
        Database.Database.SaveMessage(this);
    }
    public static Message Create(string content, Channel channel, User author)
    {
        var message = new Message(null)
        {
            Channel = channel,
            Author = author,
            Content = HttpUtility.HtmlEncode(content)
        };

        channel.Messages.Insert(0, message);
        Database.Database.SaveMessage(message);
        
        return message;
    }
}