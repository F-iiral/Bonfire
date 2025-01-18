import {SendMessageContext} from "./SendMessageContext.js";

export class SendMessageConfirmationContext extends SendMessageContext
{
    public MessageId: number;

    constructor(
        channelId: number,
        message: string | null,
        messageId: number,
    ) {
        super(channelId, message);
        this.MessageId = messageId;
    }
}