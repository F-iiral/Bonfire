import {User} from "./User";
import {Channel} from "./Channel";

export class Message {
    public Id: number;
    
    public Channel: Channel;
    public Author: User;
    public Content: string | null;
    
    constructor(
        id: number,
        channel: Channel,
        author: User,
        content: string | null
    ) {
        this.Id = id;
        this.Channel = channel;
        this.Author = author;
        this.Content = content;
    }
}