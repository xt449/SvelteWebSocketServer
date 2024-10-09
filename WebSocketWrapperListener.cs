using Newtonsoft.Json.Linq;
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
		private readonly Dictionary<string, List<ObjectSetEvent>> objectHandlers = new Dictionary<string, List<ObjectSetEvent>>();

		private readonly List<(Regex regex, BooleanRegexSetEvent handler)> booleanRegexHandlers = new List<(Regex regex, BooleanRegexSetEvent handler)>();
		private readonly List<(Regex regex, NumberRegexSetEvent handler)> numberRegexHandlers = new List<(Regex regex, NumberRegexSetEvent handler)>();
		private readonly List<(Regex regex, StringRegexSetEvent handler)> stringRegexHandlers = new List<(Regex regex, StringRegexSetEvent handler)>();
		private readonly List<(Regex regex, ObjectRegexSetEvent handler)> objectRegexHandlers = new List<(Regex regex, ObjectRegexSetEvent handler)>();

		public delegate void BooleanRegexSetEvent(string scope, Match match, bool value);
		public delegate void NumberRegexSetEvent(string scope, Match match, float value);
		public delegate void StringRegexSetEvent(string scope, Match match, string value);
		public delegate void ObjectRegexSetEvent(string scope, Match match, JObject value);

		public WebSocketWrapperListener(WebSocketWrapper wsw)
		{
			wsw.OnBooleanSet += OnBooleanSet;
			wsw.OnNumberSet += OnNumberSet;
			wsw.OnStringSet += OnStringSet;
			wsw.OnObjectSet += OnObjectSet;
		}

		private void OnBooleanSet(string scope, string id, bool value)
		{
			if (booleanHandlers.TryGetValue(id, out List<BooleanSetEvent> handlerList))
			{
				foreach (BooleanSetEvent handler in handlerList)
				{
					handler(scope, id, value);
				}
			}

			foreach ((Regex regex, BooleanRegexSetEvent handler) in booleanRegexHandlers)
			{
				Match match = regex.Match(id);

				if (match.Success)
				{
					handler(scope, match, value);
				}
			}
		}

		private void OnNumberSet(string scope, string id, float value)
		{
			if (numberHandlers.TryGetValue(id, out List<NumberSetEvent> handlerList))
			{
				foreach (NumberSetEvent handler in handlerList)
				{
					handler(scope, id, value);
				}
			}

			foreach ((Regex regex, NumberRegexSetEvent handler) in numberRegexHandlers)
			{
				Match match = regex.Match(id);

				if (match.Success)
				{
					handler(scope, match, value);
				}
			}
		}

		private void OnStringSet(string scope, string id, string value)
		{
			if (stringHandlers.TryGetValue(id, out List<StringSetEvent> handlerList))
			{
				foreach (StringSetEvent handler in handlerList)
				{
					handler(scope, id, value);
				}
			}

			foreach ((Regex regex, StringRegexSetEvent handler) in stringRegexHandlers)
			{
				Match match = regex.Match(id);

				if (match.Success)
				{
					handler(scope, match, value);
				}
			}
		}

		private void OnObjectSet(string scope, string id, JObject value)
		{
			if (objectHandlers.TryGetValue(id, out List<ObjectSetEvent> handlerList))
			{
				foreach (ObjectSetEvent handler in handlerList)
				{
					handler(scope, id, value);
				}
			}

			foreach ((Regex regex, ObjectRegexSetEvent handler) in objectRegexHandlers)
			{
				Match match = regex.Match(id);

				if (match.Success)
				{
					handler(scope, match, value);
				}
			}
		}

		// Publics

		public void AddBooleanHandler(string id, BooleanSetEvent handler)
		{
			if (booleanHandlers.TryGetValue(id, out List<BooleanSetEvent> handlerList))
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
			if (numberHandlers.TryGetValue(id, out List<NumberSetEvent> handlerList))
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
			if (stringHandlers.TryGetValue(id, out List<StringSetEvent> handlerList))
			{
				handlerList.Add(handler);
			}
			else
			{
				stringHandlers[id] = new List<StringSetEvent>() { handler };
			}
		}

		public void AddObjectHandler(string id, ObjectSetEvent handler)
		{
			if (objectHandlers.TryGetValue(id, out List<ObjectSetEvent> handlerList))
			{
				handlerList.Add(handler);
			}
			else
			{
				objectHandlers[id] = new List<ObjectSetEvent>() { handler };
			}
		}

		public void AddBooleanRegexHandler(Regex regex, BooleanRegexSetEvent handler) => booleanRegexHandlers.Add((regex, handler));

		public void AddNumberRegexHandler(Regex regex, NumberRegexSetEvent handler) => numberRegexHandlers.Add((regex, handler));

		public void AddStringRegexHandler(Regex regex, StringRegexSetEvent handler) => stringRegexHandlers.Add((regex, handler));

		public void AddObjectRegexHandler(Regex regex, ObjectRegexSetEvent handler) => objectRegexHandlers.Add((regex, handler));
	}
}
