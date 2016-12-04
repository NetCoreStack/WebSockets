using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    internal static class ConnectionManagerExtensions
    {
        public static async Task Handshake(this IConnectionManager manager, WebSocket webSocket)
        {
            WebSocketTransport transport = new WebSocketTransport(webSocket);

            var context = new WebSocketMessageContext
            {
                Command = WebSocketCommands.Handshake,
                Value = transport.ConnectionId
            };

            await manager.SendAsync(transport.ConnectionId, context, webSocket);
            await transport.Echo();
            if (webSocket.State == WebSocketState.Closed)
            {
                manager.CloseConnection(transport.ConnectionId);
            }
        }
    }
}
