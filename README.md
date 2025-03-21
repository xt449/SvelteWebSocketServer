# SvelteWebSocketServer

Basic C# implementation of a WebSocket server for use with [svelte-websocket-stores](https://github.com/xt449/svelte-websocket-stores)

[![nuget version](https://img.shields.io/nuget/v/SvelteWebSocketServer.svg)](https://www.nuget.org/packages/SvelteWebSocketServer) [![license](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## Usage

Basic Initialization
```cs
using EmbedIO;
using SvelteWebSocketServer;

var wsw = new WebSocketWrapper(); // Main entry-point
var ws = new WebServer().WithModule(webSocketWrapper);
```

Initialization using the WebSocketWrapperListener for easy event handling
```cs
using EmbedIO;
using SvelteWebSocketServer;

var wsw = new WebSocketWrapper(); // Main entry-point
var ws = new WebServer().WithModule(webSocketWrapper);
var wswListener = new WebSocketWrapperListener(wsw);
```

Basic event handling
```cs
wsw.OnJsonSet += (scope, id, value) => ...
```

Event handling using WebSocketWrapperListener
```cs
// Raw JSON value with exact ID match
wswListener.AddHandler("some.state", (scope, id, value) => ... );
// Typed value with exact ID match
wswListener.AddHandler<float>("some.state", (scope, match, value) => ... );

// These regex handlers are now deprecated. Please just use wsw.OnJsonSet instead.

// Raw JSON value with regex match
wswListener.AddHandler(new Regex("some\\.(.+?)\\.state"), (scope, id, value) => ... );
// Typed value with regex match
wswListener.AddHandler<float>(new Regex("some\\.(.+?)\\.state"), (scope, match, value) => ... );
```

## WebSocket Message Format

All communication between a client and this library is over WebSocket.

All WebSocket messages are interpreted as JSON objects.

The message object is defined as:
```ts
type Json = boolean | number | string | { [key: string]: Json } | Json[] | null;
type Message = {
	scope: string;
	id: string;
	value: Json;
}
```
The field `scope` identifies the scope of the client it comes from ~~and limits which clients receive it when coming from the server~~.
The field `id` is the primary identifier and determines where the `value` field is stored.

### Server 

#### Client Connected
1. Send the client messages for all the variables currently stored values

#### Message Received
1. The incoming text data is parsed as JSON.
2. The variable is indexed by the object's `scope` and `id` fields from the dictionary holding the respectively typed variables.
3. The variable's value is assigned to the object's `value` field.
4. Send all clients a message for the new value.
5. Handle any events.

### Client [^1]

#### Message Received
1. The incoming text data is parsed as JSON.
2. The object's `scope` field is checked if it is global ("global") or matches the client's local scope (for example "tp1"). If it does not match, the message is discarded.
3. The local Svelte store is indexed by the object's `id` field from the dictionary holding the respectively typed stores.
4. The store's value is assigned to the object's `value` field.

[^1]: This is the behavior expected by this WebSocket server
