using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public class ConnectionManager : IConnectionManager
    {
        protected IStreamCompressor Compressor { get; }
        protected ConcurrentDictionary<string, WebSocketTransport> Connections { get; }

        public ConnectionManager(IStreamCompressor compressor)
        {
            Compressor = compressor;
            Connections = new ConcurrentDictionary<string, WebSocketTransport>();
        }

        private async Task<byte[]> PrepareBytesAsync(byte[] input, JsonObject properties)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var props = JsonConvert.SerializeObject(properties);
            var propsBytes = Encoding.UTF8.GetBytes($"{props}{SocketsConstants.Splitter}");

            var bytesCount = input.Length;
            input = propsBytes.Concat(input).ToArray();

            return await Compressor.CompressAsync(input);

            //if (input.Length > SocketsConstants.CompressorThreshold)
            //{
            //    using (MemoryStream ms = new MemoryStream(input))
            //    {
            //        return await Compressor.CompressAsync(ms);
            //    }
            //}
            //else
            //    return input;           
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

        public async Task BroadcastBinaryAsync(byte[] inputs, JsonObject properties)
        {
            if (!Connections.Any())
            {
                return;
            }
            
            var bytes = await PrepareBytesAsync(inputs, properties);
            var buffer = new byte[SocketsConstants.ChunkSize];

            using (var ms = new MemoryStream(bytes))
            {
                using (var br = new BinaryReader(ms))
                {
                    byte[] chunkedBytes = null;
                    do
                    {
                        chunkedBytes = br.ReadBytes(SocketsConstants.ChunkSize);
                        var endOfMessage = false;

                        if (chunkedBytes.Length < SocketsConstants.ChunkSize)
                            endOfMessage = true;

                        foreach (var connection in Connections)
                        {
                            await SendBinaryAsync(connection.Value, chunkedBytes, endOfMessage, CancellationToken.None);
                        }

                        if (endOfMessage)
                            break;

                    } while (chunkedBytes.Length <= SocketsConstants.ChunkSize);
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

        public async Task SendBinaryAsync(string connectionId, byte[] input, JsonObject properties)
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
                transport.Dispose();
            }
        }
    }
}
