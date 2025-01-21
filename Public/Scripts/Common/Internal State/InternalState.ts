import {Server} from "../Server.js";
import {Channel} from "../Channel.js";
import {User} from "../User.js";
import {Message} from "../Message.js";
import {SelfUserContent} from "../../Content/Account/SelfUserContent.js";
import {DeleteMessageContext} from "../../Context/Channel/DeleteMessageContext.js";
import {SendMessageConfirmationContext} from "../../Context/Channel/SendMessageConfirmationContext.js";
import {EditMessageConfirmationContext} from "../../Context/Channel/EditMessageConfirmationContext.js";
import {getWebsocket, getWebsocketListeners} from "../Server/WebsocketConnection.js";

export class InternalState {
    private static Instance: InternalState;
    
    // Typescript is stupid and doesn't understand singletons (or I am stupid I suppose :3)
    public User!: User;
    public Email!: string;
    public Nicknames!: Map<Server, string>;
    public Servers!: Map<number, Server>;
    public Friends!: Map<number, User>;
    public FriendRequests!: Map<number, User>;
    public DirectMessages!: Map<number, Channel>;
    
    constructor(
        selfUser: SelfUserContent,
    ) {
        if (!(InternalState.Instance == null && InternalState.Instance == undefined)) {
            return InternalState.Instance
        }
        
        this.User = selfUser.User;
        this.Email = selfUser.Email;
        this.Nicknames = selfUser.Nicknames;
        this.Servers = new Map();
        this.Friends = new Map();
        this.FriendRequests = new Map();
        this.DirectMessages = new Map();
        
        selfUser.Friends.map(x => this.Friends.set(x.Id, x));
        selfUser.FriendRequests.map(x => this.FriendRequests.set(x.Id, x));
        selfUser.DirectMessages.map(x => this.DirectMessages.set(x.Id, x));
        
        for (const server of selfUser.Servers.values()) {
            server.Channels = new Map<number, Channel>();
        }
        selfUser.Servers.map(x => this.Servers.set(x.Id, x));
        
        for (const serverChannel of selfUser.Channels) {
            let id = serverChannel.Server?.Id ?? 0;
            
            for (const server of this.Servers.values()) {
                if (server.Id === id) {
                    server.Channels.set(serverChannel.Id, serverChannel);
                    serverChannel.Server = server;
                }
            }
        }
        InternalState.Instance = this;
    }
    public static getInstance(): InternalState {
        if (!(InternalState.Instance == null && InternalState.Instance == undefined)) {
            return InternalState.Instance
        }
        throw new Error("InternalState-Instance was not set when getInstance was called.")
    }
    public static getDebugInstance(): Object {
        return {
            "Instance": InternalState.getInstance(),
            "Listeners": getWebsocketListeners(),
            "Websocket": getWebsocket(),
        }
    }

    public findServerById(serverId: number): Server|null {
        const inst = InternalState.Instance;
        let searchedServer: Server|null = null;

        if (inst.Servers.has(serverId)) {
            searchedServer = inst.Servers.get(serverId) ?? null;
        }

        return searchedServer;
    }
    public findChannelById(channelId: number, serverId: number|null=null, includeDMs: boolean=true): Channel|null {
        const inst = InternalState.Instance;
        let searchedChannel: Channel|null = null;
        
        if (serverId != null) {
            let server = inst.findServerById(serverId) ?? null;
            if (server) {
                for (let channel of server.Channels.keys()) {
                    if (channel === channelId) {
                        searchedChannel = server.Channels.get(channel) ?? null;
                        break;
                    }
                }
            }
        }
        else {
            for (let server of inst.Servers.values()) {
                for (let channel of server.Channels.keys()) {
                    if (channel === channelId) {
                        searchedChannel = server.Channels.get(channel) ?? null;
                        break;
                    }
                }
                if (searchedChannel != null) {
                    break;
                }
            }
        }
        
        if (searchedChannel == null && includeDMs) {
            for (let channel of inst.DirectMessages.keys()) {
                if (channel === channelId) {
                    searchedChannel = inst.DirectMessages.get(channel) ?? null;
                    break;
                }
            }
        }
        return searchedChannel;
    }
    public findMessageById(messageId: number, channelId: number|null=null, serverId: number|null=null, includeDMs: boolean=true): Message|null {
        const inst = InternalState.Instance;
        let searchedMessage: Message|null = null;
        
        if (channelId != null) {
            let channel = inst.findChannelById(channelId, serverId) ?? null;
            if (channel) {
                for (let message of channel.Messages.keys()) {
                    if (message === messageId) {
                        searchedMessage = channel.Messages.get(message) ?? null;
                        break;
                    }
                }
            }
        }
        else {
            for (let server of inst.Servers.values()) {
                for (let channel of server.Channels.values()) {
                    if (channel.Id === channelId) {
                        for (let message of channel.Messages.keys()) {
                            if (message === messageId) {
                                searchedMessage = channel.Messages.get(message) ?? null;
                                break;
                            }
                        }
                    }
                    if (searchedMessage != null) {
                        break;
                    }
                }
                if (searchedMessage != null) {
                    break;
                }
            }
        }
        
        if (searchedMessage == null && includeDMs) {
            for (let channel of inst.DirectMessages.values()) {
                if (channel.Id === channelId) {
                    for (let message of channel.Messages.keys()) {
                        if (message === messageId) {
                            searchedMessage = channel.Messages.get(message) ?? null;
                            break;
                        }
                    }
                }
                if (searchedMessage != null) {
                    break;
                }
            }
        }
        return searchedMessage;
    }
    public findUserById(userId: number, includeFriends: boolean=true): User|null {
        const inst = InternalState.Instance;
        let searchedUser: User|null = null;

        for (let server of inst.Servers.values()) {
            for (let user of server.Users.keys()) {
                if (user === userId) {
                    searchedUser = server.Users.get(user) ?? null;
                    break;
                }
            }
            if (searchedUser != null) {
                break;
            }
        }

        if (searchedUser == null && includeFriends) {
            for (let friend of inst.Friends.keys()) {
                if (friend === userId) {
                    searchedUser = inst.Friends.get(friend) ?? null;
                    break;
                }
            }
        }
        return searchedUser;
    }

    public addMessage(ctx: SendMessageConfirmationContext): void {
        const inst = InternalState.Instance;

        let channel = inst.findChannelById(ctx.ChannelId);
        if (!channel)
            return;

        let author = inst.findUserById(ctx.AuthorId);
        if (!author)
            return;

        let message = new Message(ctx.MessageId, channel, author, ctx.Message);
        channel.Messages.set(message.Id, message);
    }
    public editMessage(ctx: EditMessageConfirmationContext): void {
        const inst = InternalState.Instance;

        let message = inst.findMessageById(ctx.MessageId, ctx.ChannelId, null, true);
        if (!message)
            return;

        message.Content = ctx.Message;
    }
    public deleteMessage(ctx: DeleteMessageContext): void {
        const inst = InternalState.Instance;
        
        let channel = inst.findChannelById(ctx.ChannelId, null, true);
        if (!channel)
            return;
        
        for (const message of channel.Messages.keys()) {
            if (message == ctx.MessageId) {
                let foundMessage = channel.Messages.get(message) ?? null;
                if (!foundMessage) 
                    return;
                
                let index = channel.Messages.delete(message);
            }
        }
    }
}