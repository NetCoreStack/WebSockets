using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NetCoreStack.WebSockets.Internal.NCSConstants;

namespace NetCoreStack.WebSockets
{
    public abstract class ConnectionManager : IConnectionManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHandshakeStateTransport _initState;
        private readonly IHeaderProvider _headerProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IStreamCompressor _compressor;

        public abstract InvocatorContext InvocatorContext { get; }

        public ConcurrentDictionary<string, WebSocketTransport> Connections { get; }

        public ConnectionManager(IServiceProvider serviceProvider,
            IStreamCompressor compressor,
            IHandshakeStateTransport initState,
            IHeaderProvider headerProvider,
            ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _compressor = compressor;
            _initState = initState;
            _headerProvider = headerProvider;
            _loggerFactory = loggerFactory;            
            Connections = new ConcurrentDictionary<string, WebSocketTransport>(StringComparer.OrdinalIgnoreCase);
        }

        private async Task<byte[]> PrepareFramesBytesAsync(byte[] body, IDictionary<string, object> properties)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }                

            bool compressed = GZipHelper.IsGZipBody(body);

            object key = null;
            if (properties.TryGetValue(CompressedKey, out key))
            {
                properties[CompressedKey] = compressed;
            }   
            else
            {
                properties.Add(CompressedKey, compressed);
            }

            _headerProvider.Invoke(properties);
            string props = JsonConvert.SerializeObject(properties);
            byte[] header = Encoding.UTF8.GetBytes($"{props}");

            if (!compressed)
            {
                body = await _compressor.CompressAsync(body);
            }

            body = header.Concat(Splitter).Concat(body).ToArray();
            return body;
        }

        private async Task SendMessageAsync(WebSocket webSocket, byte[] bytes, WebSocketMessageType messageType)
        {
            if (bytes == null)
            {
                return;
            }

            var length = bytes.Length;
            if (length < ChunkSize)
            {
                var segments = new ArraySegment<byte>(bytes, 0, length);
                if (!webSocket.CloseStatus.HasValue)
                {
                    await webSocket.SendAsync(segments,
                       messageType,
                       true,
                       CancellationToken.None);
                }

                return;
            }

            using (var ms = new MemoryStream(bytes))
            {
                using (var br = new BinaryReader(ms))
                {
                    byte[] chunkedBytes = null;
                    do
                    {
                        chunkedBytes = br.ReadBytes(ChunkSize);
                        var endOfMessage = false;

                        if (chunkedBytes.Length < ChunkSize)
                            endOfMessage = true;

                        var segments = new ArraySegment<byte>(chunkedBytes);

                        if (!webSocket.CloseStatus.HasValue)
                        {
                            await webSocket.SendAsync(segments,
                               messageType,
                               endOfMessage,
                               CancellationToken.None);
                        }

                        if (endOfMessage)
                            break;

                    } while (chunkedBytes.Length <= ChunkSize);
                }
            }
        }

        private async Task BroadcastMessageAsync(byte[] bytes, WebSocketMessageType messageType)
        {
            if (bytes == null)
            {
                return;
            }

            var length = bytes.Length;
            if (length < ChunkSize)
            {
                var segments = new ArraySegment<byte>(bytes, 0, length);

                foreach (var connection in Connections)
                {
                    var webSocket = connection.Value.WebSocket;
                    if (!webSocket.CloseStatus.HasValue)
                    {
                        await webSocket.SendAsync(segments,
                           messageType,
                           true,
                           CancellationToken.None);
                    }
                }

                return;
            }

            using (var ms = new MemoryStream(bytes))
            {
                using (var br = new BinaryReader(ms))
                {
                    byte[] chunkedBytes = null;
                    do
                    {
                        chunkedBytes = br.ReadBytes(ChunkSize);
                        var endOfMessage = false;

                        if (chunkedBytes.Length < ChunkSize)
                            endOfMessage = true;

                        var segments = new ArraySegment<byte>(chunkedBytes);

                        foreach (var connection in Connections)
                        {
                            var webSocket = connection.Value.WebSocket;

                            if (!webSocket.CloseStatus.HasValue)
                            {
                                await webSocket.SendAsync(segments,
                                   messageType,
                                   endOfMessage,
                                   CancellationToken.None);
                            }
                        }

                        if (endOfMessage)
                            break;

                    } while (chunkedBytes.Length <= ChunkSize);
                }
            }
        }

        public async Task ConnectAsync(WebSocket webSocket, string connectionId, string connectorName = "")
        {
            var receiverContext = new WebSocketReceiverContext
            {
                Compressor = _compressor,
                ConnectionId = connectionId,
                LoggerFactory = _loggerFactory,
                WebSocket = webSocket,
                InvocatorContext = InvocatorContext
            };
            
            WebSocketTransport transport = null;
            if (Connections.TryGetValue(connectionId, out transport))
            {
                transport.ReConnect(webSocket);
            }
            else
            {
                transport = new WebSocketTransport(webSocket, connectionId, connectorName);
                var context = new WebSocketMessageContext();
                context.Command = WebSocketCommands.Handshake;
                context.Value = connectionId;
                context.Header = await _initState.GetStateAsync();
                Connections.TryAdd(connectionId, transport);

                await SendAsync(connectionId, context);
            }

            var receiver = new WebSocketReceiver(_serviceProvider, receiverContext, CloseConnection);
            await receiver.ReceiveAsync();
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

            if (!Connections.Any())
            {
                return;
            }

            await BroadcastMessageAsync(context.ToBytes(), WebSocketMessageType.Text);
        }

        public async Task BroadcastBinaryAsync(byte[] inputs, IDictionary<string, object> properties)
        {
            if (!Connections.Any())
            {
                return;
            }
            
            var bytes = await PrepareFramesBytesAsync(inputs, properties);
            await BroadcastMessageAsync(bytes, WebSocketMessageType.Binary);
        }

        public Task SendAsync(string connectionId, WebSocketMessageContext context)
        {
            if (!Connections.Any())
            {
                return Task.CompletedTask;
            }

            if (!Connections.TryGetValue(connectionId, out WebSocketTransport transport))
            {
                throw new ArgumentOutOfRangeException(nameof(transport));
            }

            _headerProvider.Invoke(context.Header);
            return SendMessageAsync(transport.WebSocket, context.ToBytes(), WebSocketMessageType.Text);
        }

        public async Task SendBinaryAsync(string connectionId, byte[] input, IDictionary<string, object> properties)
        {
            if (!Connections.Any())
            {
                return;
            }

            if (!Connections.TryGetValue(connectionId, out WebSocketTransport transport))
            {
                throw new ArgumentOutOfRangeException(nameof(transport));
            }

            byte[] bytes = await PrepareFramesBytesAsync(input, properties);

            await SendMessageAsync(transport.WebSocket, bytes, WebSocketMessageType.Binary);
        }

        public void CloseConnection(string connectionId, bool keepAlive)
        {
            WebSocketTransport transport = null;
            if (keepAlive)
            {
                if (Connections.TryGetValue(connectionId, out transport))
                {
                    transport.Dispose();
                }
            }
            else
            {
                if (Connections.TryRemove(connectionId, out transport))
                {
                    transport.Dispose();
                }
            }
        }

        public void CloseConnection(WebSocketReceiverContext context)
        {
            bool keepAlive = false;
            if (context.WebSocket.CloseStatus == WebSocketCloseStatus.EndpointUnavailable)
            {
                keepAlive = true;
            }

            CloseConnection(context.ConnectionId, keepAlive);
        }
    }
}
