const websocket: WebSocket = new WebSocket("/websocket");

export function connectWebsocket(): void {}
export function closeWebsocket(code: number): void {
    websocket.close(code);
}

export function sendWebsocketMessage(message: string): void {
    if (websocket.readyState === WebSocket.OPEN) {
        websocket.send(message);
    }
}

websocket.addEventListener("open", async () => {
    console.log("WebSocket connection established");
});
websocket.addEventListener("message", (event) => {
    
});
websocket.addEventListener("error", (error) => {
    console.error("WebSocket error:", error);
});
websocket.addEventListener("close", (event) => {
    console.log(`WebSocket closed: code=${event.code}, reason=${event.reason}`);
});