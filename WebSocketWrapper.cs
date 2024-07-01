﻿using EmbedIO.WebSockets;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SvelteWebSocketServer
{
	public class WebSocketWrapper : WebSocketModule
	{
		private readonly Dictionary<string, bool> booleans = new Dictionary<string, bool>();
		private readonly Dictionary<string, float> numbers = new Dictionary<string, float>();
		private readonly Dictionary<string, string> strings = new Dictionary<string, string>();

		public delegate void BooleanSetEvent(string id, bool value);
		public delegate void NumberSetEvent(string id, float value);
		public delegate void StringSetEvent(string id, string value);

		public event BooleanSetEvent OnBooleanSet;
		public event NumberSetEvent OnNumberSet;
		public event StringSetEvent OnStringSet;

		public WebSocketWrapper() : base("/", true)
		{
		}

		// Handlers

		protected override async Task OnClientConnectedAsync(IWebSocketContext context)
		{
			foreach (var kvp in booleans)
			{
				await SendAsync(context, BuildBooleanMessage(kvp.Key, kvp.Value));
			}
			foreach (var kvp in numbers)
			{
				await SendAsync(context, BuildNumberMessage(kvp.Key, kvp.Value));
			}
			foreach (var kvp in strings)
			{
				await SendAsync(context, BuildStringMessage(kvp.Key, kvp.Value));
			}
		}

		protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
		{
			if (result.MessageType == (int)WebSocketMessageType.Text)
			{
				JObject jsonObject;
				try
				{
					jsonObject = JObject.Parse(Encoding.UTF8.GetString(buffer, 0, result.Count));
				}
				catch
				{
					// Abort on invalid json object
					return;
				}

				// Abort on invalid json object
				if (jsonObject["type"] == null || jsonObject["id"] == null || jsonObject["value"] == null)
				{
					return;
				}

				switch ((string)jsonObject["type"])
				{
					case "boolean":
						{
							await SetBooleanAsync((string)jsonObject["id"], (bool)jsonObject["value"]);
							break;
						}
					case "number":
						{
							await SetNumberAsync((string)jsonObject["id"], (float)jsonObject["value"]);
							break;
						}
					case "string":
						{
							await SetStringAsync((string)jsonObject["id"], (string)jsonObject["value"]);
							break;
						}
				}
			}
		}

		// Helpers

		private string BuildBooleanMessage(string id, bool value)
		{
			return $"{{\"id\":\"{id}\",\"type\":\"boolean\",\"value\":{(value ? "true" : "false")}}}";
		}

		private string BuildNumberMessage(string id, float value)
		{
			return $"{{\"id\":\"{id}\",\"type\":\"number\",\"value\":{value}}}";
		}

		private string BuildStringMessage(string id, string value)
		{
			return $"{{\"id\":\"{id}\",\"type\":\"string\",\"value\":\"{value}\"}}";
		}

		// Accessors

		public bool GetBoolean(string id)
		{
			booleans.TryGetValue(id, out var value);
			return value;
		}

		public async Task SetBooleanAsync(string id, bool value)
		{
			booleans[id] = value;
			await BroadcastAsync(BuildBooleanMessage(id, value));

			OnBooleanSet?.Invoke(id, value);
		}

		public float GetNumber(string id)
		{
			numbers.TryGetValue(id, out var value);
			return value;
		}

		public async Task SetNumberAsync(string id, float value)
		{
			numbers[id] = value;
			await BroadcastAsync(BuildNumberMessage(id, value));

			OnNumberSet?.Invoke(id, value);
		}

		public string GetString(string id)
		{
			strings.TryGetValue(id, out var value);
			return value;
		}

		public async Task SetStringAsync(string id, string value)
		{
			strings[id] = value;
			await BroadcastAsync(BuildStringMessage(id, value));

			OnStringSet?.Invoke(id, value);
		}
	}
}