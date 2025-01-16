export class User {
    public Id: number;
    
    public Name: string;
    public Discriminator: number;
    public Avatar?: string | null;
    public Banner?: string | null;
    public Bio?: string | null;
    public Status: number;
    public Flags: number;
    
    constructor(id: number, 
                name: string, 
                discriminator: number, 
                flags: number,
                status: number,
                avatar?: string | null,
                banner?: string | null,
                bio?: string | null,
    ) {
        this.Id = id;
        this.Name = name;
        this.Discriminator = discriminator;
        this.Avatar = avatar;
        this.Banner = banner;
        this.Bio = bio;
        this.Status = status;
        this.Flags = flags;
    }
}

export class ServerUser extends User {
    public Nickname: string | null;
    public PermissionLevel: number;

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
        
        this.Nickname = nickname;
        this.PermissionLevel = permissionLevel;
    }
}