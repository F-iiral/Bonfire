import {Server} from "../Server.js";
import {Channel} from "../Channel.js";
import {User} from "../User.js";
import {SelfUserContent} from "../../Content/Account/SelfUserContent.js";

export class InternalState {
    public User: User;
    public Email: string;
    public Nicknames: Map<Server, string>;
    public Servers: Server[];
    public Friends: User[];
    public FriendRequests: User[];
    public DirectMessages: Channel[];

    constructor(
        selfUser: SelfUserContent,
    ) {
        this.User = selfUser.User;
        this.Email = selfUser.Email;
        this.Nicknames = selfUser.Nicknames;
        this.Friends = selfUser.Friends;
        this.FriendRequests = selfUser.FriendRequests;
        this.DirectMessages = selfUser.DirectMessages;
        
        for (const server of selfUser.Servers) {
            server.Channels = [];
        }
        this.Servers = selfUser.Servers;
        
        for (const serverChannel of selfUser.Channels) {
            let id = serverChannel.Server?.Id ?? 0;
            
            for (const server of this.Servers) {
                if (server.Id === id) {
                    console.log(server.Id, id)
                    server.Channels.push(serverChannel);
                    serverChannel.Server = server;
                }
            }
        }
    }
}