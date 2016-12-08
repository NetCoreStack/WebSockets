using NetCoreStack.WebSockets.Common;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
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
            await Receive(webSocket);
            if (webSocket.State == WebSocketState.Closed)
            {
                manager.CloseConnection(transport.ConnectionId);
            }
        }

        private static async Task Receive(WebSocket webSocket)
        {
            var buffer = new byte[SocketsConstants.ChunkSize];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                string content = "<<binary>>";
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    content = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (content.Equals("ServerClose"))
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing from Server", CancellationToken.None);
                    }
                    else if (content.Equals("ServerAbort"))
                    {
                        webSocket.Abort();
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
