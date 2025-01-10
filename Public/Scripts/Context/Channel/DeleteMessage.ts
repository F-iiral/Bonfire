export class DeleteMessageContext
{
    public channelId: string;
    public messageId: string;

    constructor(
        channelId: string,
        messageId: string,
    ) {
        this.channelId = channelId;
        this.messageId = messageId;
    }
}