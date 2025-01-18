import {Server} from "../Server.js";
import {Channel} from "../Channel.js";
import {User} from "../User.js";
import {SelfUserContent} from "../../Content/Account/SelfUserContent.js";
import {DeleteMessageContext} from "../../Context/Channel/DeleteMessageContext.js";

export class InternalState {
    private static Instance: InternalState;
    
    // Typescript is stupid and doesn't understand singletons (or I am stupid I suppose :3)
    public User!: User;
    public Email!: string;
    public Nicknames!: Map<Server, string>;
    public Servers!: Server[];
    public Friends!: User[];
    public FriendRequests!: User[];
    public DirectMessages!: Channel[];
    
    constructor(
        selfUser: SelfUserContent,
    ) {
        if (!(InternalState.Instance == null && InternalState.Instance == undefined)) {
            return InternalState.Instance
        }
        
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
        InternalState.Instance = this;
    }
    public static getInstance(): InternalState {
        if (!(InternalState.Instance == null && InternalState.Instance == undefined)) {
            return InternalState.Instance
        }
        throw new Error("InternalState-Instance was not set when getInstance was called.")
    }
    
    public findChannelById(channelId: number, includeDMs: boolean=true): Channel|null {
        const inst = InternalState.Instance;
        let searchedChannel: Channel|null = null;
        
        for (let server of inst.Servers) {
            for (let channel of server.Channels) {
                if (channel.Id === channelId) {
                    searchedChannel = channel;
                    break;
                }
            }
            if (searchedChannel != null) {
                break;
            }
        }

        if (searchedChannel == null && includeDMs) {
            for (let channel of inst.DirectMessages) {
                if (channel.Id === channelId) {
                    searchedChannel = channel;
                    break;
                }
            }
        }
        return searchedChannel;
    }
    
    public deleteMessage(ctx: DeleteMessageContext): void {
        const inst = InternalState.Instance;
        let channel = inst.findChannelById(ctx.ChannelId);
        
        if (!channel)
            return;
        
        console.log(channel.Messages);
        for (const message of channel.Messages) {
            if (message.Id == ctx.MessageId) {
                let index = channel.Messages.indexOf(message)
                channel.Messages.splice(index, 1);
            }
        }
        console.log(channel.Messages);
    }
}