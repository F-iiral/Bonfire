using System.Web;
using BonfireServer.Database;
using BonfireServer.Internal.Context.Channel;

namespace BonfireServer.Internal.Common;

public class Message(LiteFlakeId? id) : ICachableType
{
    public LiteFlakeId Id { get; } = id ?? new LiteFlakeId();
    public Channel Channel { get; set; }
    public User Author { get; set; }
    
    public string? Content { get; set; } = null;

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