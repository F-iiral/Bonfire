import asyncio
import websockets

async def connect_to_websocket():
    url = "ws://localhost:8000/websocket"
    
    try:
        # Establish the WebSocket connection
        async with websockets.connect(url) as websocket:
            print(f"Connected to {url}\n")
            
            i = 0
            while True:
                message = f"{i}".encode("UTF-8")
                await websocket.send(message)
                print(f"Sent: {message}")
                
                response = await websocket.recv()
                response = response.decode("UTF-8")
                print(f"Received: {response}\n")
                i += 1
                await asyncio.sleep(1)
    except Exception as e:
        print(f"Error connecting to WebSocket: {e}")

asyncio.get_event_loop().run_until_complete(connect_to_websocket())