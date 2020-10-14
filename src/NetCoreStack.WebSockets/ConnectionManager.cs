using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public abstract class ConnectionManager : IConnectionManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHandshakeStateTransport _initState;
        private readonly IHeaderProvider _headerProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ConnectionManager> _logger;
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
            _logger = _loggerFactory.CreateLogger<ConnectionManager>();
            Connections = new ConcurrentDictionary<string, WebSocketTransport>(StringComparer.OrdinalIgnoreCase);
        }

        private async Task<byte[]> ToBytesAsync(byte[] body, IDictionary<string, object> properties = null)
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

            if (properties.TryGetValue(NCSConstants.CompressedKey, out object key))
            {
                properties[NCSConstants.CompressedKey] = compressed;
            }
            else
            {
                properties.Add(NCSConstants.CompressedKey, compressed);
            }

            _headerProvider.Invoke(properties);
            string props = JsonSerializer.Serialize(properties);
            byte[] header = Encoding.UTF8.GetBytes(props);

            if (!compressed)
            {
                body = await _compressor.CompressAsync(body);
            }

            body = header.Concat(NCSConstants.Splitter).Concat(body).ToArray();
            return body;
        }

        private List<Task> CreateTasks(ArraySegment<byte> segments,
            WebSocketMessageType messageType,
            bool endOfMessage,
            params string[] connections)
        {
            return connections.Select(c =>
            {
                if (Connections.TryGetValue(c, out WebSocketTransport transport))
                {
                    if (transport.WebSocket.State == WebSocketState.Aborted || 
                        transport.WebSocket.CloseStatus.HasValue)
                    {
                        Connections.TryRemove(c, out WebSocketTransport removed);
                        return Task.CompletedTask;
                    }

                    return transport.WebSocket.SendAsync(segments,
                               messageType,
                               endOfMessage,
                               CancellationToken.None);
                }

                return Task.CompletedTask;

            }).ToList();
        }

        private async Task SendDataAsync(Stream stream, 
            WebSocketMessageType messageType,
            params string[] connections)
        {            
            using (var br = new BinaryReader(stream, Encoding.UTF8))
            {
                int chunkedLength = 0;
                byte[] chunkedBytes = null;
                do
                {
                    chunkedBytes = br.ReadBytes(NCSConstants.ChunkSize);
                    chunkedLength = chunkedBytes.Length;
                    var endOfMessage = false;

                    if (chunkedLength < NCSConstants.ChunkSize)
                        endOfMessage = true;

                    var segments = new ArraySegment<byte>(chunkedBytes, 0, chunkedLength);

                    await Task.WhenAll(CreateTasks(segments, messageType, endOfMessage, connections));

                    if (endOfMessage)
                        break;

                } while (chunkedLength <= NCSConstants.ChunkSize);
            }
        }

        public async Task ConnectAsync(WebSocket webSocket, 
            string connectionId, 
            string connectorName = "", 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken != CancellationToken.None)
            {
                cancellationToken.Register(() =>
                {
                    CancellationGraceful();
                });
            }

            var receiverContext = new WebSocketReceiverContext
            {
                Compressor = _compressor,
                ConnectionId = connectionId,
                LoggerFactory = _loggerFactory,
                WebSocket = webSocket,
                InvocatorContext = InvocatorContext
            };

            if (Connections.TryGetValue(connectionId, out WebSocketTransport transport))
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

            var receiver = new WebSocketReceiver(_serviceProvider, receiverContext, CloseConnection, _loggerFactory);
            await receiver.ReceiveAsync(cancellationToken);
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

            using (var stream = context.ToMemoryStream())
            {
                await SendDataAsync(stream, WebSocketMessageType.Text, Connections.Select(c => c.Key).ToArray());
            }
        }

        public async Task BroadcastBinaryAsync(byte[] inputs, IDictionary<string, object> properties = null)
        {
            if (!Connections.Any())
            {
                return;
            }
            
            var bytes = await ToBytesAsync(inputs, properties);
            using (var stream = new MemoryStream(bytes))
            {
                await SendDataAsync(stream, WebSocketMessageType.Binary, Connections.Select(c => c.Key).ToArray());
            }
        }

        public async Task BroadcastAsync(byte[] inputs)
        {
            if (!Connections.Any())
            {
                return;
            }

            using (var stream = new MemoryStream(inputs))
            {
                await SendDataAsync(stream, WebSocketMessageType.Text, Connections.Select(c => c.Key).ToArray());
            }
        }

        public async Task BroadcastBinaryAsync(WebSocketMessageContext context)
        {
            if (!Connections.Any())
            {
                return;
            }

            using (var ms = context.ToMemoryStream())
            {
                var bytes = await ToBytesAsync(ms.ToArray());
                using (var stream = new MemoryStream(bytes))
                {
                    await SendDataAsync(stream, WebSocketMessageType.Binary, Connections.Select(c => c.Key).ToArray());
                }
            }
        }

        public async Task SendAsync(string connectionId, WebSocketMessageContext context)
        {
            if (!Connections.Any())
            {
                return;
            }

            _headerProvider.Invoke(context.Header);
            using (var stream = context.ToMemoryStream())
            {
                await SendDataAsync(stream, WebSocketMessageType.Text, connectionId);
            }
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

            byte[] bytes = await ToBytesAsync(input, properties);
            using (var stream = new MemoryStream(bytes))
            {
                await SendDataAsync(stream, WebSocketMessageType.Binary, connectionId);
            }
        }

        public void CancellationGraceful()
        {
            foreach (KeyValuePair<string, WebSocketTransport> entry in Connections)
            {
                var transport = entry.Value;
                _logger.LogInformation("Graceful cancellation. Close the websocket transport for: {0}", transport.ConnectorName);
            }
        }

        public void CloseConnection(string connectionId)
        {
            if (Connections.TryRemove(connectionId, out WebSocketTransport transport))
            {
                transport.WebSocket.Abort();
            }
        }

        public void CloseConnection(WebSocketReceiverContext context)
        {
            CloseConnection(context.ConnectionId);
        }
    }
}
