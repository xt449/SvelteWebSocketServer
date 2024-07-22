using EmbedIO;

namespace SvelteWebSocketServer
{
	public class WebSocketServer
	{
		private readonly WebServer ws;

		public readonly WebSocketWrapper webSocketWrapper;

		public WebSocketServer()
		{
			webSocketWrapper = new WebSocketWrapper();
			ws = new WebServer(50080).WithModule(webSocketWrapper);
		}

		public void Start()
		{
			ws.Start();
		}

		public void Stop()
		{
			ws.Listener.Stop();
		}
	}
}
