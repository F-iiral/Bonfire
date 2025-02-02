export class EditMessageContext
{
    public ChannelId: number;
    public MessageId: number;
    public Message: string | null;

    constructor(
        channelId: number,
        messageId: number,
        message: string | null,
    ) {
        this.ChannelId = channelId;
        this.MessageId = messageId;
        this.Message = message;
    }
}