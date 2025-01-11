import {User} from "./User";
import {Channel} from "./Channel";

export class Message {
    get id(): number {
        return this._id;
    }
    protected _id: number;
    
    public channel: Channel;
    public author: User;
    public content: string | null;
    
    constructor(
        id: number,
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