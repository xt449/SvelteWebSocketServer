﻿using EmbedIO.WebSockets;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SvelteWebSocketServer
{
	public class WebSocketWrapper : WebSocketModule
	{
		public delegate void JsonSetHandler(string scope, string id, JsonElement value);

		private readonly ConcurrentDictionary<(string scope, string id), string> rawJsonStringsDictionary = new();

		public event JsonSetHandler? OnJsonSet;

		public WebSocketWrapper() : base("/", true)
		{
		}

		// Handlers

		protected override async Task OnClientConnectedAsync(IWebSocketContext context)
		{
			// On client connect, send all current stored values
			foreach (var kvp in rawJsonStringsDictionary)
			{
				await SendAsync(context, BuildMessageRaw(kvp.Key.scope, kvp.Key.id, kvp.Value));
			}
		}

		protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
		{
			// Only handle text-type messages
			if (result.MessageType == (int)WebSocketMessageType.Text)
			{
				JsonElement rootElement;
				try
				{
					rootElement = JsonDocument.Parse(Encoding.UTF8.GetString(buffer, 0, result.Count)).RootElement;
				}
				catch
				{
					// Abort on invalid json
					return;
				}

				if (!rootElement.TryGetProperty("scope", out var scopeElement))
				{
					// Abort on missing property
					return;
				}

				var scope = scopeElement.GetString();

				if (scope == null)
				{
					// Abort on null value
					return;
				}

				if (!rootElement.TryGetProperty("id", out var idElement))
				{
					// Abort on missing property
					return;
				}

				var id = idElement.GetString();

				if (id == null)
				{
					// Abort on null value
					return;
				}

				if (!rootElement.TryGetProperty("value", out var valueElement))
				{
					// Abort on missing property
					return;
				}

				var rawJsonString = valueElement.GetRawText();

				// Set value locally
				rawJsonStringsDictionary[(scope, id)] = rawJsonString;

				// Distribute message to clients
				await BroadcastAsync(BuildMessageRaw(scope, id, rawJsonString));

				// Trigger event
				OnJsonSet?.Invoke(scope, id, valueElement);
			}
		}

		// Helpers

		private static string BuildMessageRaw(string scope, string id, string rawjsonString)
		{
			return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"value\":{rawjsonString}}}";
		}

		// Accessors

		/// <summary>
		/// Attempts to retrieve a stored value
		/// </summary>
		public bool TryGetValue<T>(string scope, string id, [MaybeNullWhen(false)] out T? value)
		{
			if (rawJsonStringsDictionary.TryGetValue((scope, id), out var jsonString))
			{
				value = JsonSerializer.Deserialize<T>(jsonString);
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Retrieves a stored value, throwing an exception if it does not exist.
		/// </summary>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the value does not exist.</exception>
		public T? GetValue<T>(string scope, string id)
		{
			return JsonSerializer.Deserialize<T>(rawJsonStringsDictionary[(scope, id)]);
		}

		/// <summary>
		/// Stores value and sends to all clients
		/// </summary>
		public async Task SetValueAsync<T>(string scope, string id, T value)
		{
			var rawJsonString = JsonSerializer.Serialize(value);

			// Set value locally
			rawJsonStringsDictionary[(scope, id)] = rawJsonString;

			// Distribute message to clients
			await BroadcastAsync(BuildMessageRaw(scope, id, rawJsonString));
		}

		/// <summary>
		/// Retrieves value, returns false if it does not exist.
		/// Applies updater function.
		/// Store new value and sends to all clients.
		/// </summary>
		public async Task<bool> TryUpdateValueAsync<T, U>(string scope, string id, Func<T?, T> updater)
		{
			// Retrieve the existing value
			if (!TryGetValue<T>(scope, id, out var existingValue))
			{
				return false;
			}

			// Store the updated value
			await SetValueAsync(scope, id, updater(existingValue));

			return true;
		}
	}
}
