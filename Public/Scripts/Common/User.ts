export class User {
    get id(): string {
        return this._id;
    }
    protected _id: string;
    
    public name: string;
    public discriminator: number;
    public avatar?: string | null;
    public banner?: string | null;
    public flags: number;
    
    constructor(id: string, 
                name: string, 
                discriminator: number, 
                flags: number, 
                avatar?: string | null,
                banner?: string | null
    ) {
        this._id = id;
        this.name = name;
        this.discriminator = discriminator;
        this.avatar = avatar;
        this.banner = banner;
        this.flags = flags;
    }
}

export class SelfUser extends User {
    public nicknames: Map<string, string>;
    public servers: string[];
    public friends: string[];
    public friendRequests: string[];
    public directMessages: string[];
    
    constructor(id: string,
                name: string,
                discriminator: number,
                flags: number,
                nicknames: Map<string, string>,
                servers: string[],
                friends: string[],
                friendsRequests: string[],
                directMessages: string[],
                avatar?:string | null,
                banner?: string | null
    ) {
        super(id, name, discriminator, flags, avatar, banner);
        
        this.nicknames = nicknames;
        this.servers = servers;
        this.friends = friends;
        this.friendRequests = friendsRequests;
        this.directMessages = directMessages;
    }
}

export class ServerUser extends User {
    public nickname: string | null;
    public permissionLevel: number;

    constructor(id: string,
                name: string,
                discriminator: number,
                flags: number,
                nickname: string | null,
                permissionLevel: number,
                avatar?: string | null,
                banner?: string | null
    ) {
        super(id, name, discriminator, flags, avatar, banner);
        
        this.nickname = nickname;
        this.permissionLevel = permissionLevel;
    }
}