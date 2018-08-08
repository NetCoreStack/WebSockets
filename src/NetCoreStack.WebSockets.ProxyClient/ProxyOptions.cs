using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ProxyOptions<TInvocator> : SocketsOptions<TInvocator> where TInvocator : IClientWebSocketCommandInvocator
    {
        public string ConnectorName { get; set; }
        public string WebSocketHostAddress { get; set; }
        public Func<WebSocket, Task> OnConnectedAsync { get; set; }
        public Func<WebSocket, Task> OnDisconnectedAsync { get; set; }

        public ProxyOptions()
        {
            ConnectorName = "";
        }
    }
}