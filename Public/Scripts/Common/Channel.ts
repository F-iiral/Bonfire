import {Message} from "./Message";
import {Server} from "./Server";

export class Channel {
    public Id: number;
    
    public Name: string;
    public Server: Server | null;
    public Messages: Message[];
    
    constructor(
        id: number,
        name: string,
        server: Server | null, 
        messages: Message[]
    ) {
        this.Id = id;
        this.Name = name;
        this.Server = server;
        this.Messages = messages;
    }
}