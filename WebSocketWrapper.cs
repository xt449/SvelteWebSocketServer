namespace SvelteWebSocketServer;

using EmbedIO.WebSockets;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class WebSocketWrapper : WebSocketModule
{
	/// <summary>
	/// Cache the values sent by the server so that new clients connecting can be instantly updated.
	/// <br/>Key: store ID.
	/// <br/>Value: store value as raw JSON string.
	/// </summary>
	private readonly ConcurrentDictionary<(string scope, string id), string> rawJsonStringStoresDictionary = new();

	public event JsonSetHandler? OnJsonSet;

	public WebSocketWrapper() : base("/", true)
	{
	}

	// Handlers

	protected override async Task OnClientConnectedAsync(IWebSocketContext context)
	{
		// On client connect, send all cached values
		foreach (((string scope, string id), string value) in rawJsonStringStoresDictionary)
		{
			await SendAsync(context, BuildMessageRaw(scope, id, value));
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

			if (!rootElement.TryGetProperty("scope", out JsonElement scopeElement))
			{
				// Abort on missing property
				return;
			}

			string? scope = scopeElement.GetString();

			if (scope == null)
			{
				// Abort on null value
				return;
			}

			if (!rootElement.TryGetProperty("id", out JsonElement idElement))
			{
				// Abort on missing property
				return;
			}

			string? id = idElement.GetString();

			if (id == null)
			{
				// Abort on null value
				return;
			}

			if (!rootElement.TryGetProperty("value", out JsonElement valueElement))
			{
				// Abort on missing property
				return;
			}

			// Trigger event
			OnJsonSet?.Invoke(scope, id, valueElement);
		}
	}

	// Accessors

	public async Task SendValueAsync<T>(string scope, string id, T value)
	{
		string rawJsonString = JsonSerializer.Serialize(value);

		// Cache value for new/reconnecting clients
		rawJsonStringStoresDictionary[(scope, id)] = rawJsonString;

		// Distribute message to clients
		await BroadcastAsync(BuildMessageRaw(scope, id, rawJsonString));
	}

	public async Task SendGlobalValueAsync<T>(string id, T value) => await SendValueAsync("global", id, value);

	public T? GetCachedOutgoingValue<T>(string scope, string id)
	{
		return JsonSerializer.Deserialize<T>(rawJsonStringStoresDictionary[(scope, id)]);
	}

	public T? GetCachedOutgoingGlobalValue<T>(string id) => GetCachedOutgoingValue<T>("global", id);

	// Helpers

	public delegate void JsonSetHandler(string scope, string id, JsonElement value);

	private static string BuildMessageRaw(string scope, string id, string rawjsonString)
	{
		return $"{{\"scope\":\"{scope}\",\"id\":\"{id}\",\"value\":{rawjsonString}}}";
	}
}
