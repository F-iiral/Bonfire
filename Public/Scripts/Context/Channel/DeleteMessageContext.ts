export class DeleteMessageContext
{
    public ChannelId: number;
    public MessageId: number;

    constructor(
        channelId: number,
        messageId: number,
    ) {
        this.ChannelId = channelId;
        this.MessageId = messageId;
    }
}