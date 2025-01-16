import {ServerUser, User} from "./User";
import {Channel} from "./Channel";

export class Server {
    public Id: number;
    
    public Name: string;
    public Owner: ServerUser;
    public Channels: Channel[];
    public Admins: ServerUser[];
    public Users: ServerUser[];
    
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
        this.Channels = channels;
        this.Admins = admins;
        this.Users = users;
    }
}