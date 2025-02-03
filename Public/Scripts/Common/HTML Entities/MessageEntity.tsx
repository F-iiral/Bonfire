import { parseFormattedText} from "../Helpers/Markdown.js";
import {InternalState} from "../Internal State/InternalState.js";
import {DeleteMessageContext} from "../../Context/Channel/DeleteMessageContext.js";
import {EditMessageConfirmationContext} from "../../Context/Channel/EditMessageConfirmationContext.js";
import ReactDOM from "react-dom";

export function createMessageElement(author: string, content: string, id: number, align = "left") {
    const container = document.createElement("div");
    container.id = id.toString();
    container.className = `message-container ${align === "right" ? "align-right" : "align-left"}`;

    const authorElement = document.createElement("div");
    authorElement.className = "message-author";
    authorElement.textContent = author;

    const messageContent = document.createElement("div");
    messageContent.className = "message-content";
    ReactDOM.render(parseFormattedText(content), messageContent);

    // Buttons container
    const buttonsContainer = document.createElement("div");
    buttonsContainer.className = "button-container"

    const deleteButton = document.createElement("button");
    deleteButton.className = "button";
    deleteButton.innerHTML = '<span class="material-symbols-outlined" style="font-size: 16px;">delete</span>';
    deleteButton.onclick = () => {
        const channelId = 0; // ToDo
        const messageId = id;
        InternalState.getInstance().deleteMessage(new DeleteMessageContext(channelId, messageId));
    };

    const editButton = document.createElement("button");
    editButton.className = "button";
    editButton.innerHTML = '<span class="material-symbols-outlined" style="font-size: 16px;">edit</span>'
    editButton.onclick = () => {
        const channelId = 0; // ToDo
        const messageId = id;
        const newMessage = ""; // ToDo
        const time = new Date().getTime();
        InternalState.getInstance().editMessage(new EditMessageConfirmationContext(channelId, messageId, newMessage, time));
    };

    buttonsContainer.appendChild(deleteButton);
    buttonsContainer.appendChild(editButton);

    container.appendChild(authorElement);
    container.appendChild(messageContent);
    container.appendChild(buttonsContainer);

    return container;
}