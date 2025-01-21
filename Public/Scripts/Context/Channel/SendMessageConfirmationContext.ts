import {SendMessageContext} from "./SendMessageContext.js";

export class SendMessageConfirmationContext extends SendMessageContext
{
    public MessageId: number;
    public AuthorId: number;

    constructor(
        channelId: number,
        message: string | null,
        messageId: number,
        authorId: number,
    ) {
        super(channelId, message);
        this.MessageId = messageId;
        this.AuthorId = authorId;
    }
}