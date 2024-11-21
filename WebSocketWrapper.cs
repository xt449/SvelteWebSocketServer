using EmbedIO.WebSockets;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SvelteWebSocketServer
{
	public class WebSocketWrapper : WebSocketModule
	{
		private readonly ConcurrentDictionary<(string scope, string id), bool> booleans = new ConcurrentDictionary<(string, string), bool>();
		private readonly ConcurrentDictionary<(string scope, string id), float> numbers = new ConcurrentDictionary<(string, string), float>();
		private readonly ConcurrentDictionary<(string scope, string id), string> strings = new ConcurrentDictionary<(string, string), string>();
		private readonly ConcurrentDictionary<(string scope, string id), JObject> objects = new ConcurrentDictionary<(string, string), JObject>();

		public delegate void BooleanSetEvent(string scope, string id, bool value);
		public delegate void NumberSetEvent(string scope, string id, float value);
		public delegate void StringSetEvent(string scope, string id, string value);
		public delegate void ObjectSetEvent(string scope, string id, JObject value);

		public event BooleanSetEvent OnBooleanSet;
		public event NumberSetEvent OnNumberSet;
		public event StringSetEvent OnStringSet;
		public event ObjectSetEvent OnObjectSet;

		public WebSocketWrapper() : base("/", true)
		{
		}

		// Handlers

		protected override async Task OnClientConnectedAsync(IWebSocketContext context)
		{
			// On client connect, send all current stored values

			foreach (KeyValuePair<(string scope, string id), bool> kvp in booleans)
			{
				await SendAsync(context, BuildBooleanMessage(kvp.Key.scope, kvp.Key.id, kvp.Value));
			}

			foreach (KeyValuePair<(string scope, string id), float> kvp in numbers)
			{
				await SendAsync(context, BuildNumberMessage(kvp.Key.scope, kvp.Key.id, kvp.Value));
			}

			foreach (KeyValuePair<(string scope, string id), string> kvp in strings)
			{
				await SendAsync(context, BuildStringMessage(kvp.Key.scope, kvp.Key.id, kvp.Value));
			}

			foreach (KeyValuePair<(string scope, string id), JObject> kvp in objects)
			{
				await SendAsync(context, BuildObjectMessage(kvp.Key.scope, kvp.Key.id, kvp.Value));
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

				// Abort on invalid message object
				if (jsonObject["scope"] == null || jsonObject["type"] == null || jsonObject["id"] == null || jsonObject["value"] == null)
				{
					return;
				}

				switch ((string)jsonObject["type"])
				{
					case "boolean":
					{
						string scope = (string)jsonObject["scope"];
						string id = (string)jsonObject["id"];
						bool value = (bool)jsonObject["value"];

						await SetBooleanAsync(scope, id, value);
						// Trigger event
						OnBooleanSet?.Invoke(scope, id, value);
						break;
					}
					case "number":
					{
						string scope = (string)jsonObject["scope"];
						string id = (string)jsonObject["id"];
						float value = (float)jsonObject["value"];

						await SetNumberAsync(scope, id, value);
						// Trigger event
						OnNumberSet?.Invoke(scope, id, value);
						break;
					}
					case "string":
					{
						string scope = (string)jsonObject["scope"];
						string id = (string)jsonObject["id"];
						string value = (string)jsonObject["value"];

						await SetStringAsync(scope, id, value);
						// Trigger event
						OnStringSet?.Invoke(scope, id, value);
						break;
					}
					case "object":
					{
						string scope = (string)jsonObject["scope"];
						string id = (string)jsonObject["id"];
						JObject value = (JObject)jsonObject["value"];

						await SetObjectAsync(scope, id, value);
						// Trigger event
						OnObjectSet?.Invoke(scope, id, value);
						break;
					}
				}
			}
		}

		// Helpers

		private static string BuildBooleanMessage(string scope, string id, bool value)
		{
			return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"type\":\"boolean\",\"value\":{(value ? "true" : "false")}}}";
		}

		private static string BuildNumberMessage(string scope, string id, float value)
		{
			return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"type\":\"number\",\"value\":{value}}}";
		}

		private static string BuildStringMessage(string scope, string id, string value)
		{
			return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"type\":\"string\",\"value\":\"{value}\"}}";
		}

		private static string BuildObjectMessage(string scope, string id, JObject value)
		{
			return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"type\":\"object\",\"value\":{value.ToString(Newtonsoft.Json.Formatting.None)}}}";
		}

		// Accessors

		/// <summary>
		/// Get value or null if undefined
		/// </summary>
		public bool? GetBoolean(string scope, string id)
		{
			return booleans.TryGetValue((scope, id), out bool value) ? value : null;
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
		/// Get value or null if undefined
		/// </summary>
		public float? GetNumber(string scope, string id)
		{
			return numbers.TryGetValue((scope, id), out float value) ? value : null;
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
		/// Get value or null if undefined
		/// </summary>
		public string? GetString(string scope, string id)
		{
			return strings.TryGetValue((scope, id), out string? value) ? value : null;
		}

		/// <summary>
		/// Store value and send to clients
		/// </summary>
		public async Task SetStringAsync(string scope, string id, string value)
		{
			strings[(scope, id)] = value;
			await BroadcastAsync(BuildStringMessage(scope, id, value));
		}

		/// <summary>
		/// Get value or null if undefined
		/// </summary>
		public JObject? GetObject(string scope, string id)
		{
			return objects.TryGetValue((scope, id), out JObject? value) ? value : null;
		}

		/// <summary>
		/// Store value and send to clients
		/// </summary>
		public async Task SetObjectAsync(string scope, string id, JObject value)
		{
			objects[(scope, id)] = value;
			await BroadcastAsync(BuildObjectMessage(scope, id, value));
		}
	}
}
