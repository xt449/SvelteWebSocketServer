# SvelteWebSocketServer

Basic C# implementation of a WebSocket server for use with [svelte-websocket-stores](https://github.com/xt449/svelte-websocket-stores)

[![nuget version](https://img.shields.io/nuget/v/SvelteWebSocketServer.svg)](https://www.nuget.org/packages/SvelteWebSocketServer) [![license](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## Usage

Basic Initialization
```cs
using SvelteWebSocketServer;

var wss = new WebSocketServer();
var wsw = wss.webSocketWrapper; // Main entry-point

wss.Start(); // Non-blocking
```

Initialization using the WebSocketWrapperListener for easy event handling
```cs

using SvelteWebSocketServer;

var wss = new WebSocketServer();
var wsw = wss.webSocketWrapper; // Main entry-point
var wswListener = new WebSocketWrapperListener(wsw);

wss.Start(); // Non-blocking
```

Basic event handling
```cs
wsw.OnNumberSet += (id, value) => ...
```

Event handling using WebSocketWrapperListener
```cs
wswListener.AddBooleanHandler("tp1.some.state", (id, value) => ... );
wswListener.AddBooleanRegexHandler(new Regex("(.+?)\\.some\\.state"), (match, value) => ... );
```

## WebSocket Message Format

All communication between a client and this library is over WebSocket.

All WebSocket messages are interpreted as JSON objects.

The message object is defined as:
```ts
type Message = {
	id: string,
	type: string,
	value: boolean | number | string,
}
```
The field `type` determines how the `value` field is interpreted as well as which of the tables (booleans, numbers, or strings) the `id` field will be indexing into.

### Server 

#### Client Connected
1. Send the client messages for all the variables currently stored values

#### Message Received
1. The incoming text data is parsed as JSON.
2. The object's `type` field is switched on with the cases `"boolean"`, `"number"`, and `"string"`. If there is no match, the message is discarded.
3. Using the values from the `id` and `value` fields, set the value of the respective variable to the new value.
4. Send all clients a message for the new value.
5. Handle any events.

### Client [^1]

#### Message Received
1. The incoming text data is parsed as JSON.
2. The object's `id` field is checked if it starts with the client's local ID prefix (for example "id1.") or the global ID prefix ("global.").
3. If it does, the `id` field is rewritten without that prefix and continues. Otherwise, the message is discarded.
4. The object's `type` field is switched on with the cases `"boolean"`, `"number"`, and `"string"`. If there is no match, the message is discarded.
5. The `value` field is cast to its respective type.

[^1]: This is the behavior expected by this WebSocket client
