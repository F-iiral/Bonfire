import {addWebsocketListener, connectWebsocket, sendWebsocketMessage} from "./Common/Server/WebsocketConnection.js";
import {sleep} from "./Common/Helpers/Sleep.js";
import {DeleteMessageContext} from "./Context/Channel/DeleteMessageContext.js";
import {EditMessageConfirmationContext} from "./Context/Channel/EditMessageConfirmationContext.js";
import {SendMessageConfirmationContext} from "./Context/Channel/SendMessageConfirmationContext.js";
import {InternalState} from "./Common/Internal State/InternalState.js";
import {Get} from "./Common/Server/HttpConnections.js";
import {SelfUserContent} from "./Content/Account/SelfUserContent.js";

const accountData = await Get<SelfUserContent>("/api/v1/account/get_self_user")
if (accountData) {
    const state = new InternalState(accountData);
}

connectWebsocket();
addWebsocketListener("DeleteMessageContext", (ctx: DeleteMessageContext) => {
    InternalState.getInstance().deleteMessage(ctx);
})
addWebsocketListener("EditMessageConfirmationContext", (ctx: EditMessageConfirmationContext) => {
    InternalState.getInstance().editMessage(ctx);
})

addWebsocketListener("SendMessageConfirmationContext", (ctx: SendMessageConfirmationContext) => {
    InternalState.getInstance().addMessage(ctx);
})
console.log(InternalState.getDebugInstance())

while (true) {
    sendWebsocketMessage("ping")
    await sleep(10000);
}