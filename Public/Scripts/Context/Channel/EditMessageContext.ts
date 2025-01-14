export class EditMessageContext
{
    public channelId: string;
    public messageId: string;
    public message: string | null;

    constructor(
        channelId: string,
        messageId: string,
        message: string | null,
    ) {
        this.channelId = channelId;
        this.messageId = messageId;
        this.message = message;
    }
}