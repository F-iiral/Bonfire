import {Message} from "./Message";
import {Server} from "./Server";

export class Channel {
    get id(): string {
        return this._id;
    }
    protected _id: string;
    
    public name: string;
    public server: Server | null;
    public messages: (Message | null)[];
    
    constructor(
        id: string,
        name: string,
        server: Server | null, 
        messages: (Message | null)[]
    ) {
        this._id = id;
        this.name = name;
        this.server = server;
        this.messages = messages;
    }
}