import {User} from "./User";
import {Channel} from "./Channel";

export class Message {
    get id(): string {
        return this._id;
    }
    protected _id: string;
    
    public channel: Channel;
    public author: User;
    public content: string | null;
    
    constructor(
        id: string,
        channel: Channel,
        author: User,
        content: string | null
    ) {
        this._id = id;
        this.channel = channel;
        this.author = author;
        this.content = content;
    }
}