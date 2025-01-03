import {ServerUser, User} from "./User";
import {Channel} from "./Channel";

export class Server {
    get id(): string {
        return this._id;
    }
    protected _id: string;
    
    public name: string;
    public owner: ServerUser;
    public channels: Channel[];
    public admins: ServerUser[];
    public users: ServerUser[];
    
    constructor(
        id: string,
        name: string,
        owner: ServerUser,
        channels: Channel[],
        admins: ServerUser[],
        users: ServerUser[]
    ) {
        this._id = id;
        this.name = name;
        this.owner = owner;
        this.channels = channels;
        this.admins = admins;
        this.users = users;
    }
}