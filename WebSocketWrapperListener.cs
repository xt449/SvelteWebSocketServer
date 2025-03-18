using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using static SvelteWebSocketServer.WebSocketWrapper;

namespace SvelteWebSocketServer
{
	public class WebSocketWrapperListener
	{
		private readonly Dictionary<string, List<ValueSetEvent>> valueHandlers = new();

		private readonly List<(Regex regex, ValueRegexSetEvent handler)> valueRegexHandlers = new();

		public delegate void ValueRegexSetEvent(string scope, Match match, JsonElement jsonElement);

		public WebSocketWrapperListener(WebSocketWrapper wsw)
		{
			wsw.OnValueSet += OnValueSet;
		}

		private void OnValueSet(string scope, string id, JsonElement jsonElement)
		{
			if (valueHandlers.TryGetValue(id, out var handlers))
			{
				foreach (ValueSetEvent handler in handlers)
				{
					handler(scope, id, jsonElement);
				}
			}

			foreach ((Regex regex, ValueRegexSetEvent handler) in valueRegexHandlers)
			{
				Match match = regex.Match(id);

				if (match.Success)
				{
					handler(scope, match, jsonElement);
				}
			}
		}

		// Publics

		public void AddValueHandler(string id, ValueSetEvent handler)
		{
			if (valueHandlers.TryGetValue(id, out var handlers))
			{
				handlers.Add(handler);
			}
			else
			{
				valueHandlers[id] = [handler];
			}
		}

		public void AddValueRegexHandler(Regex regex, ValueRegexSetEvent handler) => valueRegexHandlers.Add((regex, handler));
	}
}
