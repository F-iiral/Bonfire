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
    private static readonly DatabaseCache<Channel> ChannelCache;
    private static readonly DatabaseCache<Message> MessageCache;
    private static readonly DatabaseCache<Server> ServerCache;
    private static readonly DatabaseCache<User> UserCache;
    
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
            
            Logger.Info("Creating Caches...");
            ChannelCache = new();
            MessageCache = new();
            ServerCache = new();
            UserCache = new();
            
            Logger.Info("Successfully connected to MongoDB.");
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to initialize database (Possibly not connected?): {e.Message}");
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
        if (ChannelCache.Get(channelId, out var value))
            return value!;
        
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

        ChannelCache.Add(channel);
        return channel;
    }
    
    public static Message FindMessage(LiteFlakeId messageId)
    {
        return FindMessage(messageId.Val);
    }
    public static Message FindMessage(long messageId)
    {
        if (MessageCache.Get(messageId, out var value))
            return value!;
        
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

        MessageCache.Add(message);
        return message;
    }

    public static Server FindServer(LiteFlakeId serverId)
    {
        return FindServer(serverId.Val);
    }
    public static Server FindServer(long serverId)
    {
        if (ServerCache.Get(serverId, out var value))
            return value!;
        
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

        ServerCache.Add(server);
        return server;
    }
    
    public static User FindUser(LiteFlakeId userId)
    {
        return FindUser(userId.Val);
    }
    private static User FindUser(long userId)
    {
        if (UserCache.Get(userId, out var value))
            return value!;
        
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
        
        UserCache.Add(user);
        return user;
    }
    
    public static void SaveChannel(Channel channel)
    {
        ChannelCache.Add(channel);
        
        var databaseEntry = new ChannelEntry(channel);
        var options = new ReplaceOptions { IsUpsert = true };
        ChannelCollection.ReplaceOne(x => x.Id == databaseEntry.Id, databaseEntry, options);
    }

    public static void SaveMessage(Message message)
    {
        MessageCache.Add(message);
        
        var databaseEntry = new MessageEntry(message);
        var options = new ReplaceOptions { IsUpsert = true };
        MessageCollection.ReplaceOne(x => x.Id == databaseEntry.Id, databaseEntry, options);
    }
    
    public static void SaveServer(Server server)
    {
        ServerCache.Add(server);
        
        var databaseEntry = new ServerEntry(server);
        var options = new ReplaceOptions { IsUpsert = true };
        ServerCollection.ReplaceOne(x => x.Id == databaseEntry.Id, databaseEntry, options);
    }
    
    public static void SaveUser(User user)
    {
        UserCache.Add(user);
        
        var databaseEntry = new UserEntry(user);
        var options = new ReplaceOptions { IsUpsert = true };
        UserCollection.ReplaceOne(x => x.Id == databaseEntry.Id, databaseEntry, options);
    }
}