using EmbedIO.WebSockets;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SvelteWebSocketServer
{
	public class WebSocketWrapper : WebSocketModule
	{
		private readonly Dictionary<(string scope, string id), bool> booleans = new Dictionary<(string, string), bool>();
		private readonly Dictionary<(string scope, string id), float> numbers = new Dictionary<(string, string), float>();
		private readonly Dictionary<(string scope, string id), string> strings = new Dictionary<(string, string), string>();

		public delegate void BooleanSetEvent(string scope, string id, bool value);
		public delegate void NumberSetEvent(string scope, string id, float value);
		public delegate void StringSetEvent(string scope, string id, string value);

		public event BooleanSetEvent OnBooleanSet;
		public event NumberSetEvent OnNumberSet;
		public event StringSetEvent OnStringSet;

		public WebSocketWrapper() : base("/", true)
		{
		}

		// Handlers

		protected override async Task OnClientConnectedAsync(IWebSocketContext context)
		{
			// On client connect, send all current stored values

			foreach (var kvp in booleans)
			{
				await SendAsync(context, BuildBooleanMessage(kvp.Key.scope, kvp.Key.id, kvp.Value));
			}
			foreach (var kvp in numbers)
			{
				await SendAsync(context, BuildNumberMessage(kvp.Key.scope, kvp.Key.id, kvp.Value));
			}
			foreach (var kvp in strings)
			{
				await SendAsync(context, BuildStringMessage(kvp.Key.scope, kvp.Key.id, kvp.Value));
			}
		}

		protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
		{
			// Only handle text-type messages
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
				if (jsonObject["scope"] == null || jsonObject["type"] == null || jsonObject["id"] == null || jsonObject["value"] == null)
				{
					return;
				}

				switch ((string)jsonObject["type"])
				{
					case "boolean":
						{
							var scope = (string)jsonObject["scope"];
							var id = (string)jsonObject["id"];
							var value = (bool)jsonObject["value"];

							await SetBooleanAsync(scope, id, value);
							// Trigger event
							OnBooleanSet?.Invoke(scope, id, value);
							break;
						}
					case "number":
						{
							var scope = (string)jsonObject["scope"];
							var id = (string)jsonObject["id"];
							var value = (float)jsonObject["value"];

							await SetNumberAsync(scope, id, value);
							// Trigger event
							OnNumberSet?.Invoke(scope, id, value);
							break;
						}
					case "string":
						{
							var scope = (string)jsonObject["scope"];
							var id = (string)jsonObject["id"];
							var value = (string)jsonObject["value"];

							await SetStringAsync(scope, id, value);
							// Trigger event
							OnStringSet?.Invoke(scope, id, value);
							break;
						}
				}
			}
		}

		// Helpers

		private string BuildBooleanMessage(string scope, string id, bool value)
		{
			return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"type\":\"boolean\",\"value\":{(value ? "true" : "false")}}}";
		}

		private string BuildNumberMessage(string scope, string id, float value)
		{
			return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"type\":\"number\",\"value\":{value}}}";
		}

		private string BuildStringMessage(string scope, string id, string value)
		{
			return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"type\":\"string\",\"value\":\"{value}\"}}";
		}

		// Accessors

		/// <summary>
		/// Get value or default if undefined
		/// </summary>
		public bool GetBoolean(string scope, string id)
		{
			booleans.TryGetValue((scope, id), out var value);
			return value;
		}

		/// <summary>
		/// Store value and send to clients
		/// </summary>
		public async Task SetBooleanAsync(string scope, string id, bool value)
		{
			booleans[(scope, id)] = value;
			await BroadcastAsync(BuildBooleanMessage(scope, id, value));
		}

		/// <summary>
		/// Get value or default if undefined
		/// </summary>
		public float GetNumber(string scope, string id)
		{
			numbers.TryGetValue((scope, id), out var value);
			return value;
		}

		/// <summary>
		/// Store value and send to clients
		/// </summary>
		public async Task SetNumberAsync(string scope, string id, float value)
		{
			numbers[(scope, id)] = value;
			await BroadcastAsync(BuildNumberMessage(scope, id, value));
		}

		/// <summary>
		/// Get value or default if undefined
		/// </summary>
		public string GetString(string scope, string id)
		{
			strings.TryGetValue((scope, id), out var value);
			return value;
		}

		/// <summary>
		/// Store value and send to clients
		/// </summary>
		public async Task SetStringAsync(string scope, string id, string value)
		{
			strings[(scope, id)] = value;
			await BroadcastAsync(BuildStringMessage(scope, id, value));
		}
	}
}
