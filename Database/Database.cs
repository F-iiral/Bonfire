using System.Text;
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
    private static readonly Dictionary<long, Channel> PreloadedChannels = new();
    private static readonly Dictionary<long, Message> PreloadedMessages = new();
    private static readonly Dictionary<long, Server> PreloadedServers = new();
    private static readonly Dictionary<long, User> PreloadedUsers = new();
    
    static Database()
    {
        try
        {
            Logger.Info("Opening MongoDB connection...");
            
            var client = new MongoClient();
            var db = client.GetDatabase("Bonfire");
            
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
        catch (Exception ex)
        {
            throw new Exception($"Failed to initialize database (Possibly not connected?): {ex}");
        }
    }

    public static void CreateIndexes()
    {
        var userIdIndex = new List<CreateIndexModel<UserEntry>>{new (Builders<UserEntry>.IndexKeys.Ascending(x => x.Id)) };
        var userNameIndex = new List<CreateIndexModel<UserEntry>>{new (Builders<UserEntry>.IndexKeys.Ascending(x => x.Name)) };
        var channelIndex = new List<CreateIndexModel<ChannelEntry>>{new (Builders<ChannelEntry>.IndexKeys.Ascending(x => x.Id)) };
        var serverIndex = new List<CreateIndexModel<ServerEntry>>{new (Builders<ServerEntry>.IndexKeys.Ascending(x => x.Id)) };
        var messageIndex = new List<CreateIndexModel<MessageEntry>>{new (Builders<MessageEntry>.IndexKeys.Ascending(x => x.Id)) };
      
        UserCollection.Indexes.CreateMany(userIdIndex);
        UserCollection.Indexes.CreateMany(userNameIndex);
        ChannelCollection.Indexes.CreateMany(channelIndex);
        ServerCollection.Indexes.CreateMany(serverIndex);
        MessageCollection.Indexes.CreateMany(messageIndex);
        
        Logger.Info("Successfully created MongoDB indexes.");
    }

    public static Channel? FindChannel(LiteFlakeId channelId)
    {
       return FindChannel(channelId.Val);
    }
    public static Channel? FindChannel(long channelId, Server? parsedServer=null)
    {
        if (ChannelCache.Get(channelId, out var value))
            return value!;
        if (PreloadedChannels.TryGetValue(channelId, out var preloadedChannel))
            return preloadedChannel;
        
        var expression = Builders<ChannelEntry>.Filter.Eq(x => x.Id, channelId);
        var data = ChannelCollection.Find(expression).FirstOrDefault();

        if (data == null)
            return null;

        var channel = new Channel(new LiteFlakeId(data.Id));
        PreloadedChannels[channelId] = channel;
        
        channel.Server = parsedServer ?? FindServer(data.Server);
        channel.Name = data.Name;
        //var messagesToLoad = data.Messages.Skip(Math.Max(0,  data.Messages.Count - 255));
        //channel.Messages = messagesToLoad.Select(x => FindMessage(x, channel)).Where(x => x != null).ToList()!;

        ChannelCache.Add(channel);
        return channel;
    }

    public static void TryExtendChannelMessages(Channel channel, out bool success, long beforeId = 0)
    {
        var expression = Builders<ChannelEntry>.Filter.Eq(x => x.Id, channel.Id.Val);
        var data = ChannelCollection.Find(expression).FirstOrDefault();

        if (data == null)
            throw new NullReferenceException("Channel doesn't exist in the database but is in memory.");

        const int messagesToLoadCount = 255;
        var currentLoadedCount = channel.Messages.Count;

        var messagesToLoad = beforeId != 0
            ? data.Messages.Where(x => x < beforeId).Take(messagesToLoadCount).ToList()
            : data.Messages.Skip(currentLoadedCount).Take(messagesToLoadCount).ToList();

        if (messagesToLoad.Count == 0)
            success = false;

        var newMessages = messagesToLoad.Select(x => FindMessage(x, channel)).Where(x => x != null).ToList();
        channel.Messages.AddRange(newMessages!);
        channel.Messages = channel.Messages.OrderByDescending(x => x.Id).ToList();
        success = true;
    }
    
    public static Message? FindMessage(LiteFlakeId messageId)
    {
        return FindMessage(messageId.Val);
    }
    public static Message? FindMessage(long messageId, Channel? parsedChannel=null)
    {
        if (MessageCache.Get(messageId, out var value))
            return value!;
        if (PreloadedMessages.TryGetValue(messageId, out var preloadedMessage))
            return preloadedMessage;
        
        var expression = Builders<MessageEntry>.Filter.Eq(x => x.Id, messageId);
        var data = MessageCollection.Find(expression).FirstOrDefault();

        if (data == null)
            return null;

        var channel = parsedChannel ?? FindChannel(data.Channel);
        var author = FindUser(data.Author);
  
        if (author == null || channel == null)
            return null;

        var message = new Message(new LiteFlakeId(data.Id));
        PreloadedMessages[messageId] = message;
        
        message.Channel = channel;
        message.Author = author;
        message.Content = data.Content;

        MessageCache.Add(message);
        return message;
    }

    public static Server? FindServer(LiteFlakeId serverId)
    {
        return FindServer(serverId.Val);
    }
    public static Server? FindServer(long serverId, User? parsedUser=null)
    {
        // DMs
        if (serverId == 0)
            return null;
        
        if (ServerCache.Get(serverId, out var value))
            return value!;
        if (PreloadedServers.TryGetValue(serverId, out var preloadedServer))
            return preloadedServer;
        
        var expression = Builders<ServerEntry>.Filter.Eq(x => x.Id, serverId);
        var data = ServerCollection.Find(expression).FirstOrDefault();
        
        if (data == null)
            return null;
        
        User? owner;
        if (parsedUser != null && data.Owner != parsedUser.Id)
            owner = FindUser(data.Owner);
        else
            owner = parsedUser;
        
        if (owner == null)
            return null;
        
        var server = new Server(new LiteFlakeId(data.Id));
        PreloadedServers[serverId] = server;
        
        server.Name = data.Name;
        server.Owner = owner;
        server.Channels = data.Channels.Select(x => FindChannel(x, server)).Where(x => x != null).ToList()!;

        if (parsedUser != null)
        {
            var admins = data.Admins.Select(x => x.Item1 != parsedUser.Id ? new Tuple<User?, byte>(FindUser(x.Item1), x.Item2) : new Tuple<User?, byte>(parsedUser, x.Item2) );
            server.Admins = admins.Where(x => x.Item1 is not null).ToList()!;
            server.Users =  data.Users.Select(x => x != parsedUser.Id ? FindUser(x) : parsedUser).Where(x => x != null).ToList()!;
        }
        else
        {
            var admins = data.Admins.Select(x => new Tuple<User?, byte>(FindUser(x.Item1), x.Item2));
            server.Admins = admins.Where(x => x.Item1 is not null).ToList()!;
            server.Users =  data.Users.Select(FindUser).Where(x => x != null).ToList()!;
        }

        ServerCache.Add(server);
        return server;
    }

    public static User? FindUserByToken(string token)
    {
        var splitToken = token.Split('.');
        if (splitToken.Length != 4)
            return null;
        
        var success = long.TryParse(Encoding.UTF8.GetString(Convert.FromBase64String(splitToken[0])), out var id);
        if (!success)
            return null;
        
        return FindUser(id);
    }
    public static User? FindUser(LiteFlakeId userId)
    {
        return FindUser(userId.Val);
    }
    public static User? FindUser(long userId)
    {
        if (UserCache.Get(userId, out var value))
            return value!;
        if (PreloadedUsers.TryGetValue(userId, out var preloadedUser))
            return preloadedUser;
        
        var expression = Builders<UserEntry>.Filter.Eq(x => x.Id, userId);
        var data = UserCollection.Find(expression).FirstOrDefault();
        
        if (data == null)
            return null;
        
        var user = new User(new LiteFlakeId(data.Id));
        PreloadedUsers[userId] = user;
        
        user.Name = data.Name;
        user.Discriminator = data.Discriminator;
        user.Email = data.Email;
        user.PasswordHash = data.PasswordHash;
        user.PasswordSalt = data.PasswordSalt;
        user.AuthToken = new AuthToken(data.AuthToken);
        user.Avatar = data.Avatar;
        user.Banner = data.Banner;
        user.Bio = data.Bio;
        user.Status = data.Status;
        user.Flags = data.Flags;
        user.Nicknames = data.Nicknames
            .Where(pair => FindServer(pair.Key, user) != null)
            .ToDictionary(pair => FindServer(pair.Key, user), pair => pair.Value)!;
        user.Servers = data.Servers.Select(x => FindServer(x, user)).Where(x => x != null).ToList()!;
        user.Friends = data.Friends.Select(FindUser).Where(x => x != null).ToList()!;
        user.FriendRequests = data.FriendRequests.Select(FindUser).Where(x => x != null).ToList()!;
        user.DirectMessages = data.DirectMessages.Select(x => FindChannel(x, null)).Where(x => x != null).ToList()!;
        
        UserCache.Add(user);
        return user;
    }
    
    public static void SaveChannel(Channel channel)
    {
        ChannelCache.Add(channel);
        
        var databaseEntry = new ChannelEntry(channel);
        var options = new ReplaceOptions { IsUpsert = true };
        var result = ChannelCollection.ReplaceOne(x => x.Id == databaseEntry.Id, databaseEntry, options);
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

    public static void DeleteChannel(Channel channel)
    {
        ChannelCache.Remove(channel.Id, out var _);
        
        ChannelCollection.DeleteOne(x => x.Id == channel.Id.Val);
    }
    
    public static void DeleteMessage(Message message)
    {
        MessageCache.Remove(message.Id, out var _);
        
        MessageCollection.DeleteOne(x => x.Id == message.Id.Val);
    }
    
    public static void DeleteServer(Server server)
    {
        ServerCache.Remove(server.Id, out var _);
        
        ServerCollection.DeleteOne(x => x.Id == server.Id.Val);
    }
    
    public static void DeleteUser(User user)
    {
        UserCache.Remove(user.Id, out var _);
        
        UserCollection.DeleteOne(x => x.Id == user.Id.Val);
    }
}