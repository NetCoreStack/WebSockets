using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
using static NetCoreStack.WebSockets.Internal.SocketsConstants;

namespace NetCoreStack.WebSockets
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly InvocatorRegistry _invocatorRegistry;
        private readonly ServerSocketsOptions _options;
        private readonly IHandshakeStateTransport _initState;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IStreamCompressor _compressor;
        public ConcurrentDictionary<string, WebSocketTransport> Connections { get; }

        public ConnectionManager(IStreamCompressor compressor,
            InvocatorRegistry invocatorRegistry,
            IOptions<ServerSocketsOptions> options,
            IHandshakeStateTransport initState,
            ILoggerFactory loggerFactory)
        {
            _invocatorRegistry = invocatorRegistry;
            _options = options.Value;
            _initState = initState;
            _loggerFactory = loggerFactory;
            _compressor = compressor;
            Connections = new ConcurrentDictionary<string, WebSocketTransport>();
        }

        private async Task<byte[]> PrepareBytesAsync(byte[] input, IDictionary<string, object> properties)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (properties == null)
                properties = new Dictionary<string, object>();

            bool compressed = GZipHelper.IsGZipHeader(input);

            object key = null;
            if (properties.TryGetValue(CompressedKey, out key))
                properties[CompressedKey] = compressed;
            else
                properties.Add(CompressedKey, compressed);

            string props = JsonConvert.SerializeObject(properties);
            byte[] header = Encoding.UTF8.GetBytes($"{props}{Splitter}");

            if (compressed)
                header = await _compressor.CompressAsync(header);

            input = header.Concat(input).ToArray();

            if (!compressed)
                return await _compressor.CompressAsync(input);

            return input;
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

        private async Task SendBinaryAsync(WebSocketTransport transport, byte[] chunkedBytes, bool endOfMessage, CancellationToken token)
        {
            if (transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }

            var segments = new ArraySegment<byte>(chunkedBytes);

            await transport.WebSocket.SendAsync(segments,
                           WebSocketMessageType.Binary,
                           endOfMessage,
                           token);
        }

        public async Task ConnectAsync(WebSocket webSocket)
        {
            WebSocketTransport transport = new WebSocketTransport(webSocket);
            var connectionId = transport.ConnectionId;
            var context = new WebSocketMessageContext();
            context.Command = WebSocketCommands.Handshake;
            context.Value = connectionId;
            context.State = await _initState.GetStateAsync();
            Connections.TryAdd(connectionId, transport);

            await SendAsync(connectionId, context);

            var receiverContext = new WebSocketReceiverContext
            {
                Compressor = _compressor,
                ConnectionId = connectionId,
                InvocatorRegistry = _invocatorRegistry,
                LoggerFactory = _loggerFactory,
                Options = _options,
                WebSocket = webSocket
            };
            var receiver = new WebSocketReceiver(receiverContext, CloseConnection);
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

        public async Task BroadcastBinaryAsync(byte[] inputs, IDictionary<string, object> properties)
        {
            if (!Connections.Any())
            {
                return;
            }
            
            var bytes = await PrepareBytesAsync(inputs, properties);

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

                        foreach (var connection in Connections)
                        {
                            await SendBinaryAsync(connection.Value, chunkedBytes, endOfMessage, CancellationToken.None);
                        }

                        if (endOfMessage)
                            break;

                    } while (chunkedBytes.Length <= ChunkSize);
                }
            }
        }

        public async Task SendAsync(string connectionId, WebSocketMessageContext context)
        {
            if (!Connections.Any())
            {
                return;
            }

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

        public async Task SendBinaryAsync(string connectionId, byte[] input, IDictionary<string, object> properties)
        {
            if (!Connections.Any())
            {
                return;
            }

            WebSocketTransport transport = null;
            if (!Connections.TryGetValue(connectionId, out transport))
            {
                throw new ArgumentOutOfRangeException(nameof(transport));
            }

            byte[] bytes = await PrepareBytesAsync(input, properties);

            using (var ms = new MemoryStream(bytes))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    byte[] chunkBytes = null;
                    do
                    {
                        chunkBytes = br.ReadBytes(ChunkSize);
                        var segments = new ArraySegment<byte>(chunkBytes);
                        var endOfMessage = false;

                        if (chunkBytes.Length < ChunkSize)
                            endOfMessage = true;

                        await transport.WebSocket.SendAsync(segments, 
                            WebSocketMessageType.Binary, 
                            endOfMessage, 
                            CancellationToken.None);

                        if (endOfMessage)
                            break;

                    } while (chunkBytes.Length <= ChunkSize);
                }
            }
        }

        public void CloseConnection(string connectionId)
        {
            WebSocketTransport transport = null;
            if (Connections.TryRemove(connectionId, out transport))
            {
                transport.Dispose();
            }
        }

        public void CloseConnection(WebSocketReceiverContext context)
        {
            CloseConnection(context.ConnectionId);
        }
    }
}
