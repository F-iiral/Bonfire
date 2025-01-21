import {getCookie} from "../Helpers/Cookies.js";

const websocket: WebSocket = new WebSocket("/websocket");
const responseListeners: Map<string, Function[]> = new Map([
    ["DeleteMessageContext", []],
    ["EditMessageConfirmationContext", []],
    ["SendMessageConfirmationContext", []],
]);

class Event {
    EventType: string;
    Payload: any;

    constructor(
        eventType: string,
        payload: any,
    ) {
        this.EventType = eventType;
        this.Payload = payload;
    }
}

export function connectWebsocket(): void {}
export function closeWebsocket(code: number): void {
    websocket.close(code);
}

export function getWebsocket(): WebSocket {
    return websocket;
}
export function getWebsocketListeners(): Map<string, Function[]> {
    return responseListeners;
}
export function sendWebsocketMessage(message: string): void {
    if (websocket.readyState === WebSocket.OPEN) {
        websocket.send(message);
    }
}
export function addWebsocketListener(eventType: string, func: Function) {
    if (responseListeners.has(eventType)) {
        responseListeners.get(eventType)!.push(func);
    }
    else {
        console.error(`Tried to register unknown event type ${eventType}. Are you up to date?`);
    }
}

websocket.addEventListener("open", async () => {
    console.log("WebSocket connection established.");
    sendWebsocketMessage(getCookie("token") ?? "")
});
websocket.addEventListener("message", (event) => {
    if (event.data == "ACK") 
        return;
    
    const parsedEvent = JSON.parse(event.data) as Event;
    if (responseListeners.has(parsedEvent.EventType)) {
        for (const listener of responseListeners.get(parsedEvent.EventType)!) {
            listener(parsedEvent.Payload);
        }
    }
    else {
        console.warn(`Received unknown event type ${event.type}. Are you up to date?`);
    }
});
websocket.addEventListener("error", (error) => {
    console.error("WebSocket error:", error);
});
websocket.addEventListener("close", (event) => {
    console.log(`WebSocket closed: code=${event.code}, reason=${event.reason}`);
});