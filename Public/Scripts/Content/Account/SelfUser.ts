import {User} from "../../Common/User.js";
import {Server} from "../../Common/Server.js";
import {Channel} from "../../Common/Channel.js";

export class SelfUserContent
{
    public user: User;
    public email: string;
    public nicknames: Map<Server, string>;
    public servers: Server[];
    public friends: User[];
    public friendRequests: User[];
    public directMessages: Channel[];

    constructor(
        user: User,
        email: string,
        nicknames: Map<Server, string>,
        servers: Server[],
        friends: User[],
        friendRequests: User[],
        directMessages: Channel[],
    ) {
        this.user = user;
        this.email = email;
        this.nicknames = nicknames;
        this.servers = servers;
        this.friends = friends;
        this.friendRequests = friendRequests;
        this.directMessages = directMessages;
    }
}