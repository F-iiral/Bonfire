export class GetMessagesContext
{
    public count: number;
    public channelId: string;
    public greedy: boolean;

    constructor(
        channelId: string,
        count?: number,
        greedy?: boolean,
    ) {
        this.count = count ?? 64;
        this.channelId = channelId;
        this.greedy = greedy ?? false;
    }
}