export class GetMessagesContext
{
    public Count: number;
    public ChannelId: number;
    public Greedy: boolean;
    public Before: number;

    constructor(
        channelId: number,
        count?: number,
        greedy?: boolean,
        before?: number,
    ) {
        this.Count = count ?? 64;
        this.ChannelId = channelId;
        this.Greedy = greedy ?? false;
        this.Before = before ?? 2**53;
    }
}