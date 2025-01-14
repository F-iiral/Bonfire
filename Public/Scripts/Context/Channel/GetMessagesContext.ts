export class GetMessagesContext
{
    public count: number;
    public channelId: number;
    public greedy: boolean;
    public before: number;

    constructor(
        channelId: number,
        count?: number,
        greedy?: boolean,
        before?: number,
    ) {
        this.count = count ?? 64;
        this.channelId = channelId;
        this.greedy = greedy ?? false;
        this.before = before ?? 2**53;
    }
}