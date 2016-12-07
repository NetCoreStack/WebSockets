using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
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

        public async Task SendBinaryAsync(string connectionId, byte[] bytes, JsonObject properties)
        {
            WebSocketTransport transport = null;
            if (!Connections.TryGetValue(connectionId, out transport))
            {
                throw new ArgumentOutOfRangeException(nameof(transport));
            }

            var props = JsonConvert.SerializeObject(properties);
            var propsBytes = Encoding.UTF8.GetBytes($"{SocketsConstants.Splitter}{props}");

            var bytesCount = bytes.Length;
            bytes = bytes.Concat(propsBytes).ToArray();

            var buffer = new byte[SocketsConstants.ChunkSize];
            using (var ms = new MemoryStream(bytes))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    byte[] chunkBytes = null;
                    do
                    {
                        chunkBytes = br.ReadBytes(SocketsConstants.ChunkSize);
                        var segments = new ArraySegment<byte>(chunkBytes);
                        var endOfMessage = false;

                        if (chunkBytes.Length < SocketsConstants.ChunkSize)
                            endOfMessage = true;

                        await transport.WebSocket.SendAsync(segments, 
                            WebSocketMessageType.Binary, 
                            endOfMessage, 
                            CancellationToken.None);

                        if (endOfMessage)
                            break;

                    } while (chunkBytes.Length <= SocketsConstants.ChunkSize);
                }
            }
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
