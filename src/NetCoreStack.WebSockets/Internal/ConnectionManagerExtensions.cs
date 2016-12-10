using NetCoreStack.WebSockets.Interfaces;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    internal static class ConnectionManagerExtensions
    {
        public static async Task Handshake(this IConnectionManager manager, 
            WebSocket webSocket,
            IStreamCompressor compressor,
            InvocatorRegistry invocatorRegistry,
            ServerSocketsOptions options,
            IHandshakeStateTransport initState)
        {
            WebSocketTransport transport = new WebSocketTransport(webSocket);

            var context = new WebSocketMessageContext();
            context.Command = WebSocketCommands.Handshake;
            context.Value = transport.ConnectionId;
            context.State = await initState.GetStateAsync();            

            await manager.SendAsync(transport.ConnectionId, context, webSocket);

            try
            {
                await WebSocketReceiver.Receive(webSocket, compressor, invocatorRegistry, (SocketsOptions)options);
                if (webSocket.State == WebSocketState.Aborted || webSocket.State == WebSocketState.Closed)
                {
                    manager.CloseConnection(transport.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                transport.Dispose();
            }            
        }
    }
}
