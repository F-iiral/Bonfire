import {ServerUser, User} from "./User";
import {Channel} from "./Channel";

export class Server {
    public Id: number;
    
    public Name: string;
    public Owner: ServerUser;
    public Channels: Map<number, Channel>;
    public Admins: Map<number, ServerUser>;
    public Users:  Map<number, ServerUser>;
    
    constructor(
        id: number,
        name: string,
        owner: ServerUser,
        channels: Channel[],
        admins: ServerUser[],
        users: ServerUser[]
    ) {
        this.Id = id;
        this.Name = name;
        this.Owner = owner;
        this.Channels = new Map();
        this.Admins = new Map();
        this.Users = new Map();
        
        channels.map(x => this.Channels.set(x.Id, x));
        admins.map(x => this.Admins.set(x.Id, x));
        users.map(x => this.Users.set(x.Id, x));
    }
}