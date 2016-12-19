using System;
using System.Net.WebSockets;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketTransport : IDisposable
    {
        public WebSocket WebSocket { get; }
        public string ConnectionId { get; }
        public string ConnectorName { get; }

        public WebSocketTransport(WebSocket webSocket, string connectorName)
        {
            ConnectionId = Guid.NewGuid().ToString();
            WebSocket = webSocket;
            ConnectorName = connectorName;
        }

        public void Dispose()
        {
            WebSocket.Dispose();
        }
    }
}
