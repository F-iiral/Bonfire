export class User {
    get id(): number {
        return this._id;
    }
    protected _id: number;
    
    public name: string;
    public discriminator: number;
    public avatar?: string | null;
    public banner?: string | null;
    public bio?: string | null;
    public status: number;
    public flags: number;
    
    constructor(id: number, 
                name: string, 
                discriminator: number, 
                flags: number,
                status: number,
                avatar?: string | null,
                banner?: string | null,
                bio?: string | null,
    ) {
        this._id = id;
        this.name = name;
        this.discriminator = discriminator;
        this.avatar = avatar;
        this.banner = banner;
        this.bio = bio;
        this.status = status;
        this.flags = flags;
    }
}

export class ServerUser extends User {
    public nickname: string | null;
    public permissionLevel: number;

    constructor(id: number,
                name: string,
                discriminator: number,
                flags: number,
                nickname: string | null,
                permissionLevel: number,
                status: number,
                avatar?: string | null,
                banner?: string | null,
                bio?: string | null,
    ) {
        super(id, name, discriminator, flags, status, avatar, banner, bio);
        
        this.nickname = nickname;
        this.permissionLevel = permissionLevel;
    }
}