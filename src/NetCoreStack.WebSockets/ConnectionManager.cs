using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly SemaphoreSlim _sendFrameAsyncLock = new SemaphoreSlim(1, 1);
        private readonly InvocatorRegistry _invocatorRegistry;
        private readonly ServerSocketsOptions _options;
        private readonly IHandshakeStateTransport _initState;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IStreamCompressor _compressor;
        private readonly TransportLifetimeManager _lifetimeManager;

        public ConcurrentDictionary<string, WebSocketTransport> Connections { get; }

        public ConnectionManager(IStreamCompressor compressor,
            TransportLifetimeManager lifetimeManager,
            InvocatorRegistry invocatorRegistry,
            IOptions<ServerSocketsOptions> options,
            IHandshakeStateTransport initState,
            ILoggerFactory loggerFactory)
        {
            _compressor = compressor;
            _lifetimeManager = lifetimeManager;
            _invocatorRegistry = invocatorRegistry;
            _options = options.Value;
            _initState = initState;
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
                properties = new Dictionary<string, object>();

            bool compressed = GZipHelper.IsGZipBody(body);

            object key = null;
            if (properties.TryGetValue(CompressedKey, out key))
                properties[CompressedKey] = compressed;
            else
                properties.Add(CompressedKey, compressed);

            string props = JsonConvert.SerializeObject(properties);
            byte[] header = Encoding.UTF8.GetBytes($"{props}");

#if DEBUG
            if (properties.TryGetValue("Key", out key))
            {
                int length = body.Length;
                Debug.WriteLine($"=====Key: {key?.ToString()}=====Length: {length}=====");
            }
#endif

            if (!compressed)
                body = await _compressor.CompressAsync(body);

            body = header.Concat(Splitter).Concat(body).ToArray();

            return body;
        }

        private Task SendAsync(WebSocketTransport transport, WebSocketMessageDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (descriptor.Segments == null)
            {
                throw new ArgumentNullException(nameof(descriptor.Segments));
            }

            if (!transport.WebSocket.CloseStatus.HasValue)
            {
                return transport.WebSocket.SendAsync(descriptor.Segments,
                   descriptor.MessageType,
                   descriptor.EndOfMessage,
                   CancellationToken.None);
            }
            else
            {
                // Only text messages
                _lifetimeManager.AddQueue(transport.ConnectionId, new MessageHolder
                {
                    Segments = descriptor.Segments,
                    KeepTime = DateTime.Now.AddMinutes(3)
                });

                return TaskCache.CompletedTask;
            }
        }

        private Task SendBinaryAsync(WebSocketTransport transport, byte[] chunkedBytes, bool endOfMessage)
        {
            if (transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }

            var segments = new ArraySegment<byte>(chunkedBytes);

            if (!transport.WebSocket.CloseStatus.HasValue)
            {
                return transport.WebSocket.SendAsync(segments,
                   WebSocketMessageType.Binary,
                   endOfMessage,
                   CancellationToken.None);
            }

            return TaskCache.CompletedTask;
        }

        private async Task SendConcurrentBinaryAsync(byte[] bytes)
        {
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
                            await SendBinaryAsync(connection.Value, chunkedBytes, endOfMessage);
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
                InvocatorRegistry = _invocatorRegistry,
                LoggerFactory = _loggerFactory,
                Options = _options,
                WebSocket = webSocket
            };
            
            WebSocketTransport transport = null;
            if (Connections.TryGetValue(connectionId, out transport))
            {
                transport.ReConnect(webSocket);
                List<MessageHolder> messages = _lifetimeManager.TryDequeue(connectionId);
                foreach (var message in messages)
                {
                    await SendAsync(transport, new WebSocketMessageDescriptor
                    {
                        MessageType = WebSocketMessageType.Text,
                        Segments = message.Segments,
                        EndOfMessage = true,
                        IsQueue = true,
                    });
                }
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

            _sendFrameAsyncLock.Wait();
            foreach (var connection in Connections)
            {
                await SendAsync(connection.Value, descriptor);
            }
            _sendFrameAsyncLock.Release();
        }

        public async Task BroadcastBinaryAsync(byte[] inputs, IDictionary<string, object> properties)
        {
            if (!Connections.Any())
            {
                return;
            }
            
            _sendFrameAsyncLock.Wait();
            var bytes = await PrepareFramesBytesAsync(inputs, properties);
            await SendConcurrentBinaryAsync(bytes);
            _sendFrameAsyncLock.Release();
        }

        public Task SendAsync(string connectionId, WebSocketMessageContext context)
        {
            if (!Connections.Any())
            {
                return TaskCache.CompletedTask;
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

            return SendAsync(transport, descriptor);
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

            byte[] bytes = await PrepareFramesBytesAsync(input, properties);

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
