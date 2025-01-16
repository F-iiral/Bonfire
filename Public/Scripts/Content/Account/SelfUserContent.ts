import {User} from "../../Common/User.js";
import {Server} from "../../Common/Server.js";
import {Channel} from "../../Common/Channel.js";

export class SelfUserContent
{
    public User: User;
    public Email: string;
    public Nicknames: Map<Server, string>;
    public Servers: Server[];
    public Channels: Channel[];
    public Friends: User[];
    public FriendRequests: User[];
    public DirectMessages: Channel[];

    constructor(
        user: User,
        email: string,
        nicknames: Map<Server, string>,
        servers: Server[],
        channels: Channel[],
        friends: User[],
        friendRequests: User[],
        directMessages: Channel[],
    ) {
        this.User = user;
        this.Email = email;
        this.Nicknames = nicknames;
        this.Servers = servers;
        this.Channels = channels;
        this.Friends = friends;
        this.FriendRequests = friendRequests;
        this.DirectMessages = directMessages;
    }
}