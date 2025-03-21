using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SvelteWebSocketServer
{
	public class WebSocketWrapperListener
	{
		public delegate void JsonSetHandler(string scope, string id, JsonElement value);
		public delegate void TypedSetHandler<T>(string scope, string id, T? value);

		public delegate void RegexJsonSetHandler(string scope, Match match, JsonElement value);
		public delegate void RegexTypedSetHandler<T>(string scope, Match match, T? value);

		private readonly Dictionary<string, List<JsonSetHandler>> jsonHandlers = new();
		private readonly Dictionary<string, List<Delegate>> typedHandlers = new();

		private readonly List<(Regex regex, RegexJsonSetHandler handler)> regexJsonHandlers = new();
		private readonly List<(Regex regex, Delegate handler)> regexTypedHandlers = new();

		public WebSocketWrapperListener(WebSocketWrapper wsw)
		{
			wsw.OnJsonSet += OnJsonSet;
		}

		private void OnJsonSet(string scope, string id, JsonElement jsonElement)
		{
			// Invoke handlers that process raw JSON values for exact ID matches
			if (this.jsonHandlers.TryGetValue(id, out var jsonHandlers))
			{
				foreach (JsonSetHandler handler in jsonHandlers)
				{
					handler(scope, id, jsonElement);
				}
			}

			// Invoke handlers that process deserialized values for exact ID matches
			if (this.typedHandlers.TryGetValue(id, out var typedHandlers))
			{
				foreach (Delegate handler in typedHandlers)
				{
					// Get type dynamically from handler
					Type targetType = handler.GetType().GenericTypeArguments[0];
					// Deserialize jsonElement with type
					object? deserializedValue = JsonSerializer.Deserialize(jsonElement, targetType);
					// Invoke handler
					handler.DynamicInvoke(scope, id, deserializedValue);
				}
			}

			// Invoke handlers that process raw JSON values for regex matched IDs
			foreach ((Regex regex, RegexJsonSetHandler handler) in regexJsonHandlers)
			{
				Match match = regex.Match(id);

				if (match.Success)
				{
					handler(scope, match, jsonElement);
				}
			}

			// Invoke handlers that process deserialized values for regex matched IDs
			foreach ((Regex regex, Delegate handler) in regexTypedHandlers)
			{
				Match match = regex.Match(id);

				if (match.Success)
				{
					// Get type dynamically from handler
					Type targetType = handler.GetType().GenericTypeArguments[0];
					// Deserialize jsonElement with type
					object? deserializedValue = JsonSerializer.Deserialize(jsonElement, targetType);
					// Invoke handler
					handler.DynamicInvoke(scope, match, deserializedValue);
				}
			}
		}

		// Publics

		/// <summary>
		/// Adds a handler for processing raw JSON values where there is an exact ID match.
		/// </summary>
		public void AddHandler(string id, JsonSetHandler handler)
		{
			if (jsonHandlers.TryGetValue(id, out var handlers))
			{
				handlers.Add(handler);
			}
			else
			{
				jsonHandlers[id] = [handler];
			}
		}

		/// <summary>
		/// Adds a handler for processing deserialized values where there is an exact ID match.
		/// </summary>
		public void AddHandler<T>(string id, TypedSetHandler<T> handler)
		{
			if (typedHandlers.TryGetValue(id, out var handlers))
			{
				handlers.Add(handler);
			}
			else
			{
				typedHandlers[id] = [handler];
			}
		}

		/// <summary>
		/// Adds a handler for processing raw JSON values where there is a regex match.
		/// </summary>
		[Obsolete("Use WebSocketWrapper#OnJsonSet instead")]
		public void AddHandler(Regex regex, RegexJsonSetHandler handler) => regexJsonHandlers.Add((regex, handler));

		/// <summary>
		/// Adds a handler for processing deserialized values where there is a regex match.
		/// </summary>
		[Obsolete("Use WebSocketWrapper#OnJsonSet instead")]
		public void AddHandler<T>(Regex regex, RegexTypedSetHandler<T> handler) => regexTypedHandlers.Add((regex, handler));
	}
}
