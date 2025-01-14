export class SendMessageContext
{
    public channelId: string;
    public message: string | null;

    constructor(
        channelId: string,
        message: string | null,
    ) {
        this.channelId = channelId;
        this.message = message;
    }
}