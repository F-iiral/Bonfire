import {ServerUser, User} from "./User";
import {Channel} from "./Channel";

export class Server {
    get id(): number {
        return this._id;
    }
    protected _id: number;
    
    public name: string;
    public owner: ServerUser;
    public channels: Channel[];
    public admins: ServerUser[];
    public users: ServerUser[];
    
    constructor(
        id: number,
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