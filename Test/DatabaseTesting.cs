using BonfireServer.Internal;
using BonfireServer.Internal.Common;
using BonfireServer.Internal.Const;

namespace BonfireServer.Test;

public static class DatabaseTesting
{
    public static void CreateDatabase()
    {
        Logger.Info("The Server will now create a database for testing purposes. It contains 1 server, 1 user, 50 channels and 50k Messages. This may take a moment.");
        Database.Database.CreateIndexes();
        
        var user = User.RegisterNewUser($"Debug User", "example@email.com", $"password");
        Database.Database.SaveUser(user);
        
        var server = new Server(null);
        server.Name = "Debug Server";
        
        server.Owner = user;
        server.Users.Add(user);
        server.Admins.Add(new(user, AdminLevels.Owner));

        for (var i = 0; i < 50; i++)
        {
            Logger.Info($"Starting Channel #{i}");
            var channel = new Channel(null);
            channel.Name = $"Debug Channel #{i}";
            channel.Server = server;
            server.Channels.Add(channel);
            
            for (var j = 0; j < 1000; j++)
            {
                
                var message = new Message(null);
                message.Content = $"Debug Message {j}";
                message.Author = user;
                message.Channel = channel;
            
                channel.Messages.Insert(0, message);
                Database.Database.SaveMessage(message!);
            }
            
            Database.Database.SaveChannel(channel);
        }

        Database.Database.SaveUser(user);
        Database.Database.SaveServer(server);
    }

    public static void CreateLargeDatabase()
    {
        Logger.Info("The Server will now create a database for testing purposes. It contains 10 server, 200 user, 500 channels and 5M Messages. This may take a moment.");
        Database.Database.CreateIndexes();

        var users = new List<User>();
        for (var u = 0; u < 200; u++)
        {
            var user = User.RegisterNewUser($"Debug User {u}", "example@email.com", $"password{u}");
            Database.Database.SaveUser(user);
        }

        var random = new Random();
        for (var i = 0; i < 10; i++)
        {
            var server = new Server(null);
            server.Name = $"Debug Server #{i}";
            
            server.Owner = users[random.Next(0, users.Count)];
            server.Users.Add(server.Owner);
            server.Admins.Add(new(server.Owner, AdminLevels.Owner));
            
            var additionalAdminsCount = random.Next(1, 20);
            for (var a = 0; a < additionalAdminsCount; a++)
            {
                var randomUser = users[random.Next(0, users.Count)];

                if (server.Admins.All(admin => admin.Item1 != randomUser))
                {
                    var randomAdminLevel = (byte)random.Next(AdminLevels.VerifiedMember, AdminLevels.Owner);
                    server.Admins.Add(new(randomUser, randomAdminLevel));
                }
            }
            
            for (var j = 0; j < 50; j++)
            {
                Logger.Info($"Starting Channel #{j} @ Server #{i}");
                var channel = new Channel(null);
                channel.Name = $"Debug Channel #{j}";
                channel.Server = server;
                server.Channels.Add(channel);
                
                for (var k = 0; k < 1000; k++)
                {
                    
                    var message = new Message(null);
                    message.Content = $"Debug Message {k}";
                    message.Author = users[random.Next(0, users.Count)];
                    message.Channel = channel;
                
                    channel.Messages.Insert(0, message);
                    Database.Database.SaveMessage(message!);
                }
                
                Database.Database.SaveChannel(channel);
            }
            
            Database.Database.SaveServer(server);
        }
    }

    public static void FindServerEntry(long id)
    {
        Database.Database.CreateIndexes();
        
        Logger.Info($"Beginning to a server entry. Ideally, you provided a larger server to test the database speed.");
        var server = Database.Database.FindServer(id);
        Logger.Info($"Fully loaded the server {server!.ToString()}");
    }
}