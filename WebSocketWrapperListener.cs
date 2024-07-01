using System.Collections.Generic;
using System.Text.RegularExpressions;
using static SvelteWebSocketServer.WebSocketWrapper;

namespace SvelteWebSocketServer
{
	public class WebSocketWrapperListener
	{
		private readonly Dictionary<string, List<BooleanSetEvent>> booleanHandlers = new Dictionary<string, List<BooleanSetEvent>>();
		private readonly Dictionary<string, List<NumberSetEvent>> numberHandlers = new Dictionary<string, List<NumberSetEvent>>();
		private readonly Dictionary<string, List<StringSetEvent>> stringHandlers = new Dictionary<string, List<StringSetEvent>>();

		private readonly List<(Regex regex, BooleanRegexSetEvent handler)> booleanRegexHandlers = new List<(Regex regex, BooleanRegexSetEvent handler)>();
		private readonly List<(Regex regex, NumberRegexSetEvent handler)> numberRegexHandlers = new List<(Regex regex, NumberRegexSetEvent handler)>();
		private readonly List<(Regex regex, StringRegexSetEvent handler)> stringRegexHandlers = new List<(Regex regex, StringRegexSetEvent handler)>();

		public delegate void BooleanRegexSetEvent(Match match, bool value);
		public delegate void NumberRegexSetEvent(Match match, float value);
		public delegate void StringRegexSetEvent(Match match, string value);

		public WebSocketWrapperListener(WebSocketWrapper wsw)
		{
			wsw.OnBooleanSet += OnBooleanSet;
			wsw.OnNumberSet += OnNumberSet;
			wsw.OnStringSet += OnStringSet;
		}

		private void OnBooleanSet(string id, bool value)
		{
			if (booleanHandlers.TryGetValue(id, out var handlerList))
			{
				foreach (var handler in handlerList)
				{
					handler(id, value);
				}
			}

			foreach (var (regex, handler) in booleanRegexHandlers)
			{
				var match = regex.Match(id);

				if (match.Success)
				{
					handler(match, value);
				}
			}
		}

		private void OnNumberSet(string id, float value)
		{
			if (numberHandlers.TryGetValue(id, out var handlerList))
			{
				foreach (var handler in handlerList)
				{
					handler(id, value);
				}
			}

			foreach (var (regex, handler) in numberRegexHandlers)
			{
				var match = regex.Match(id);

				if (match.Success)
				{
					handler(match, value);
				}
			}
		}

		private void OnStringSet(string id, string value)
		{
			if (stringHandlers.TryGetValue(id, out var handlerList))
			{
				foreach (var handler in handlerList)
				{
					handler(id, value);
				}
			}

			foreach (var (regex, handler) in stringRegexHandlers)
			{
				var match = regex.Match(id);

				if (match.Success)
				{
					handler(match, value);
				}
			}
		}

		// Publics

		public void AddBooleanHandler(string id, BooleanSetEvent handler)
		{
			if (booleanHandlers.TryGetValue(id, out var handlerList))
			{
				handlerList.Add(handler);
			}
			else
			{
				booleanHandlers[id] = new List<BooleanSetEvent>() { handler };
			}
		}

		public void AddNumberHandler(string id, NumberSetEvent handler)
		{
			if (numberHandlers.TryGetValue(id, out var handlerList))
			{
				handlerList.Add(handler);
			}
			else
			{
				numberHandlers[id] = new List<NumberSetEvent>() { handler };
			}
		}

		public void AddStringHandler(string id, StringSetEvent handler)
		{
			if (stringHandlers.TryGetValue(id, out var handlerList))
			{
				handlerList.Add(handler);
			}
			else
			{
				stringHandlers[id] = new List<StringSetEvent>() { handler };
			}
		}

		public void AddBooleanRegexHandler(Regex regex, BooleanRegexSetEvent handler) => booleanRegexHandlers.Add((regex, handler));

		public void AddNumberRegexHandler(Regex regex, NumberRegexSetEvent handler) => numberRegexHandlers.Add((regex, handler));

		public void AddStringRegexHandler(Regex regex, StringRegexSetEvent handler) => stringRegexHandlers.Add((regex, handler));
	}
}
