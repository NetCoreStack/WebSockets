using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public class ConnectionManager : IConnectionManager
    {
        protected ILoggerFactory LoggerFactory { get; }
        protected ConcurrentDictionary<string, WebSocketTransport> Connections { get; }

        public ConnectionManager(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory; 
            Connections = new ConcurrentDictionary<string, WebSocketTransport>();
        }

        private async Task SendAsync(WebSocketTransport transport, WebSocketMessageDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (descriptor.Segments == null)
            {
                throw new ArgumentNullException(nameof(descriptor.Segments));
            }

            await transport.WebSocket.SendAsync(descriptor.Segments, 
                descriptor.MessageType, 
                descriptor.EndOfMessage, 
                CancellationToken.None);
        }

        public async Task BroadcastAsync(WebSocketMessageContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Value == null)
            {
                throw new ArgumentNullException(nameof(context.Value));
            }

            var segments = context.ToSegment();
            var descriptor = new WebSocketMessageDescriptor
            {
                Segments = segments,
                EndOfMessage = true,
                MessageType = WebSocketMessageType.Text
            };

            foreach (var connection in Connections)
            {
                await SendAsync(connection.Value, descriptor);
            }
        }

        public async Task SendAsync(string connectionId, WebSocketMessageContext context)
        {
            WebSocketTransport transport = null;
            if (!Connections.TryGetValue(connectionId, out transport))
            {
                throw new ArgumentOutOfRangeException(nameof(transport));
            }

            var segments = context.ToSegment();
            var descriptor = new WebSocketMessageDescriptor
            {
                Segments = segments,
                EndOfMessage = true,
                MessageType = WebSocketMessageType.Text
            };

            await SendAsync(transport, descriptor);
        }

        public async Task SendAsync(string connectionId, WebSocketMessageContext context, WebSocket webSocket)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentNullException(nameof(connectionId));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (webSocket == null)
            {
                throw new ArgumentNullException(nameof(webSocket));
            }

            WebSocketTransport transport = null;
            if (!Connections.TryGetValue(connectionId, out transport))
            {
                Connections.TryAdd(connectionId, new WebSocketTransport(webSocket));
            }

            await SendAsync(connectionId, context);
        }

        public void CloseConnection(string connectionId)
        {
            WebSocketTransport transport = null;
            if (Connections.TryRemove(connectionId, out transport))
            {
                transport.WebSocket.Dispose();
            }
        }
    }
}
