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
wsw.OnNumberSet += (scope, id, value) => ...
```

Event handling using WebSocketWrapperListener
```cs
wswListener.AddBooleanHandler("tp1", "some.state", (scope, id, value) => ... );
wswListener.AddBooleanRegexHandler("tp1", new Regex("some\\.(.+?)\\.state"), (scope, match, value) => ... );
```

## WebSocket Message Format

All communication between a client and this library is over WebSocket.

All WebSocket messages are interpreted as JSON objects.

The message object is defined as:
```ts
type Message = {
	scope: string,
	id: string,
	type: "boolean" | "number" | "string" | "object",
	value: boolean | number | string | object,
}
```
The field `scope` identifies the scope of the client it comes from and limits which clients receive it when coming from the server.
The field `type` determines how the `value` field is interpreted as well as which of the tables (booleans, numbers, strings, or objects) the `scope` and `id` fields will be indexing into.

### Server 

#### Client Connected
1. Send the client messages for all the variables currently stored values

#### Message Received
1. The incoming text data is parsed as JSON.
2. The object's `type` field is switched on with the cases `"boolean"`, `"number"`, `"string"`, and `"object"`. If there is no match, the message is discarded.
3. The variable is indexed by the object's `scope` and `id` fields from the dictionary holding the respectively typed variables.
4. The variable's value is assigned to the object's `value` field, cast to its respective type.
5. Send all clients a message for the new value.
6. Handle any events.

### Client [^1]

#### Message Received
1. The incoming text data is parsed as JSON.
2. The object's `scope` field is checked if it is global ("global") or matches the client's local scope (for example "tp1"). If it does not match, the message is discarded.
3. The object's `type` field is switched on with the cases `"boolean"`, `"number"`, `"string"`, and `"object"`. If there is no match, the message is discarded.
4. The local Svelte store is indexed by the object's `id` field from the dictionary holding the respectively typed stores.
5. The store's value is assigned to the object's `value` field, cast to its respective type.

[^1]: This is the behavior expected by this WebSocket server
