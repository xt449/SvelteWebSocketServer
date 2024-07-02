# SvelteWebSocketServer

Basic C# implementation of a WebSocket server for use with [svelte-websocket-stores](https://github.com/xt449/svelte-websocket-stores)

[![nuget version](https://img.shields.io/nuget/v/SvelteWebSocketServer.svg)](https://www.nuget.org/packages/SvelteWebSocketServer) [![license](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## Usage

```cs
using SvelteWebSocketServer;

var wss = new WebSocketServer();
var wsw = wss.webSocketWrapper; // This is for interacting with svelte-websocket-stores

wss.Start(); // Non-blocking
```
