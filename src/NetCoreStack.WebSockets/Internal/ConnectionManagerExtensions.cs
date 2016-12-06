using NetCoreStack.WebSockets.Common;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    internal static class ConnectionManagerExtensions
    {
        public static async Task Handshake(this IConnectionManager manager, 
            WebSocket webSocket, 
            IHandshakeStateTransport initState)
        {
            WebSocketTransport transport = new WebSocketTransport(webSocket);

            var context = new WebSocketMessageContext();
            context.Command = WebSocketCommands.Handshake;
            context.Value = transport.ConnectionId;
            context.State = await initState.GetStateAsync();            

            await manager.SendAsync(transport.ConnectionId, context, webSocket);
            await transport.Echo();
            if (webSocket.State == WebSocketState.Closed)
            {
                manager.CloseConnection(transport.ConnectionId);
            }
        }
    }
}
