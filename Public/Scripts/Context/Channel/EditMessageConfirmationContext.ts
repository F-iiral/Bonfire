import {EditMessageContext} from "./EditMessageContext.js";

export class EditMessageConfirmationContext extends EditMessageContext
{
    public LastEdited: number;

    constructor(
        channelId: number,
        messageId: number,
        message: string | null,
        lastEdited: number,
    ) {
        super(channelId, messageId, message);
        this.LastEdited = lastEdited;
    }
}