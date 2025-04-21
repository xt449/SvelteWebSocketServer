namespace SvelteWebSocketServer;

public class WrappedStore<T>(WebSocketWrapper wsw, string scope, string id)
{
	private readonly WebSocketWrapper wsw = wsw;

	public readonly string scope = scope;
	public readonly string id = id;

	public T? Value
	{
		get => wsw.GetCachedOutgoingValue<T>(scope, id);
		set => _ = wsw.SendValueAsync(scope, id, value);
	}
}
