using System;
using System.Net.WebSockets;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketTransport : IDisposable
    {
        public WebSocket WebSocket { get; private set; }
        public string ConnectionId { get; }
        public string ConnectorName { get; }

        public WebSocketTransport(WebSocket webSocket, string connectionId, string connectorName)
        {
            ConnectionId = connectionId;
            WebSocket = webSocket;
            ConnectorName = connectorName;
        }

        public void ReConnect(WebSocket webSocket)
        {
            WebSocket = webSocket;
        }

        public void Dispose()
        {
            WebSocket.Dispose();
        }
    }
}
