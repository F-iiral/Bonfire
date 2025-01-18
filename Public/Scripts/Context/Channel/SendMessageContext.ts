export class SendMessageContext
{
    public ChannelId: number;
    public Message: string | null;

    constructor(
        channelId: number,
        message: string | null,
    ) {
        this.ChannelId = channelId;
        this.Message = message;
    }
}