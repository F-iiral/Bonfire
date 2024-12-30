using BonfireServer.Database.DatabaseTypes;
using BonfireServer.Internal;
using BonfireServer.Internal.Common;
using MongoDB.Driver;

namespace BonfireServer.Database;

public static class Database
{
    private static readonly IMongoCollection<ChannelEntry> ChannelCollection;
    private static readonly IMongoCollection<MessageEntry> MessageCollection;
    private static readonly IMongoCollection<ServerEntry> ServerCollection;
    private static readonly IMongoCollection<UserEntry> UserCollection;
    
    static Database()
    {
        try
        {
            Logger.Info("Opening MongoDB connection...");
            
            var client = new MongoClient();
            var db = client.GetDatabase("Bonefire");
            
            Logger.Info("Fetching MongoDB connections...");
            UserCollection = db.GetCollection<UserEntry>("users");
            ChannelCollection = db.GetCollection<ChannelEntry>("channels");
            ServerCollection = db.GetCollection<ServerEntry>("servers");
            MessageCollection = db.GetCollection<MessageEntry>("messages");
            
            Logger.Info("Successfully connceted to MongoDB.");
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
            Logger.Error("\nDid you forget to connect to the database?");
        }
    }

    public static void CreateIndexes()
    {
        var userIndex = new List<CreateIndexModel<UserEntry>>{new (Builders<UserEntry>.IndexKeys.Ascending(x => x.Id)) };
        var channelIndex = new List<CreateIndexModel<ChannelEntry>>{new (Builders<ChannelEntry>.IndexKeys.Ascending(x => x.Id)) };
        var serverIndex = new List<CreateIndexModel<ServerEntry>>{new (Builders<ServerEntry>.IndexKeys.Ascending(x => x.Id)) };
        var messageIndex = new List<CreateIndexModel<MessageEntry>>{new (Builders<MessageEntry>.IndexKeys.Ascending(x => x.Id)) };
        
        UserCollection.Indexes.CreateMany(userIndex);
        ChannelCollection.Indexes.CreateMany(channelIndex);
        ServerCollection.Indexes.CreateMany(serverIndex);
        MessageCollection.Indexes.CreateMany(messageIndex);
        
        Logger.Info("Successfully created MongoDB indexes.");
    }

    public static Channel FindChannel(LiteFlakeId channelId)
    {
       return FindChannel(channelId.Val);
    }
    public static Channel FindChannel(long channelId)
    {
        var expression = Builders<ChannelEntry>.Filter.Eq(x => x.Id, channelId);
        var data = ChannelCollection.Find(expression).First();

        if (data == null)
        {
            Logger.Warn("Channel not found. This might cause an error later."); 
            return null;
        }

        var channel = new Channel(new LiteFlakeId(data.Id));
        channel.Server = FindServer(data.Id);
        channel.Name = data.Name;
        channel.Messages = data.Messages.Select(FindMessage).ToList();

        return channel;
    }
    
    public static Message FindMessage(LiteFlakeId messageId)
    {
        return FindMessage(messageId.Val);
    }
    public static Message FindMessage(long messageId)
    {
        var expression = Builders<MessageEntry>.Filter.Eq(x => x.Id, messageId);
        var data = MessageCollection.Find(expression).First();

        if (data == null)
        {
            Logger.Warn("Message not found! This might cause an error later."); 
            return null;
        }

        var message = new Message(new LiteFlakeId(data.Id));
        message.Channel = FindChannel(data.Channel);
        message.Author = FindUser(data.Author);
        message.Content = data.Content;

        return message;
    }

    public static Server FindServer(LiteFlakeId serverId)
    {
        return FindServer(serverId.Val);
    }
    public static Server FindServer(long serverId)
    {
        var expression = Builders<ServerEntry>.Filter.Eq(x => x.Id, serverId);
        var data = ServerCollection.Find(expression).First();
        
        if (data == null)
        {
            Logger.Warn("Server not found. This might cause an error later."); 
            return null;
        }

        var server = new Server(new LiteFlakeId(data.Id));
        server.Name = data.Name;
        server.Owner = FindUser(data.Owner);
        server.Channels = data.Channels.Select(FindChannel).ToList();
        server.Admins = data.Admins.Select(x => new Tuple<User, byte>(FindUser(x.Item1), x.Item2)).ToList();
        server.Users = data.Users.Select(FindUser).ToList();

        return server;
    }
    
    public static User FindUser(LiteFlakeId userId)
    {
        return FindUser(userId.Val);
    }
    private static User FindUser(long userId)
    {
        var expression = Builders<UserEntry>.Filter.Eq(x => x.Id, userId);
        var data = UserCollection.Find(expression).First();
        
        if (data == null)
        {
            Logger.Warn("User not found. This might cause an error later."); 
            return null;
        }

        var user = new User(new LiteFlakeId(data.Id));
        user.Name = data.Name;
        user.Discriminator = data.Discriminator;
        user.Email = data.Email;
        user.PasswordHash = data.PasswordHash;
        user.PasswordSalt = data.PasswordSalt;
        user.Avatar = data.Avatar;
        user.Banner = data.Banner;
        user.Flags = data.Flags;
        user.Nicknames = data.Nicknames.ToDictionary(
            pair => FindServer(pair.Key), 
            pair => pair.Value);
        user.Servers = data.Servers.Select(FindServer).ToList();
        user.Friends = data.Servers.Select(FindUser).ToList();
        user.FriendRequests = data.Servers.Select(FindUser).ToList();
        user.DirectMessages = data.Servers.Select(FindChannel).ToList();
        
        return user;
    }
    
    public static void SaveChannel(Channel message)
    {
        SaveChannel(new ChannelEntry(message));
    }
    public static void SaveChannel(ChannelEntry channelEntry)
    {    
        var options = new ReplaceOptions { IsUpsert = true };
        ChannelCollection.ReplaceOne(x => x.Id == channelEntry.Id, channelEntry, options);
    }

    public static void SaveMessage(Message message)
    {
        SaveMessage(new MessageEntry(message));
    }
    public static void SaveMessage(MessageEntry messageEntry)
    {    
        var options = new ReplaceOptions { IsUpsert = true };
        MessageCollection.ReplaceOne(x => x.Id == messageEntry.Id, messageEntry, options);
    }
    
    public static void SaveServer(Server server)
    {
        SaveServer(new ServerEntry(server));
    }
    public static void SaveServer(ServerEntry serverEntry)
    { 
        var options = new ReplaceOptions { IsUpsert = true };
        ServerCollection.ReplaceOne(x => x.Id == serverEntry.Id, serverEntry, options);
    }
    
    public static void SaveUser(User user)
    {
        SaveUser(new UserEntry(user));
    }
    public static void SaveUser(UserEntry userEntry)
    {    
        var options = new ReplaceOptions { IsUpsert = true };
        UserCollection.ReplaceOne(x => x.Id == userEntry.Id, userEntry, options);
    }
}