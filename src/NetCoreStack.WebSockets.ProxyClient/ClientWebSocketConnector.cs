using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    internal class ClientWebSocketConnector : IWebSocketConnector
    {
        private string _connectionId;
        private ClientWebSocket _webSocket;
        private readonly ConnectorOptions _options;
        private readonly InvocatorRegistry _invocatorRegistry;

        public ClientWebSocketConnector(IOptions<ConnectorOptions> options, 
            InvocatorRegistry invocatorRegistry)
        {
            _options = options.Value;
            _invocatorRegistry = invocatorRegistry;
        }

        private async Task Receive()
        {
            var buffer = new byte[SocketsConstants.ChunkSize];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var context = result.ToContext(buffer);
                    if (context.Command == WebSocketCommands.Handshake)
                        _connectionId = context.Value.ToString();

                    var _invocators = _invocatorRegistry.GetInvocators(context);
                    _invocators.ForEach(async x => await x.InvokeAsync(context));
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    byte[] binaryResult = null;
                    using (var ms = new MemoryStream())
                    {
                        while (!result.EndOfMessage)
                        {
                            if (!result.CloseStatus.HasValue)
                            {
                                await ms.WriteAsync(buffer, 0, result.Count);
                            }
                            result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        }
                        if (result.EndOfMessage)
                        {
                            if (!result.CloseStatus.HasValue)
                            {
                                await ms.WriteAsync(buffer, 0, result.Count);
                            }
                        }
                        binaryResult = ms.ToArray();
                    }
                    var context = result.ToBinaryContext(binaryResult);
                    var _invocators = _invocatorRegistry.GetInvocators(context);
                    _invocators.ForEach(async x => await x.InvokeAsync(context));
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async Task ConnectAsync()
        {
            var uri = new Uri($"ws://{_options.WebSocketHostAddress}");
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(uri, CancellationToken.None);
            await Task.WhenAll(Receive());
        }

        public async Task SendAsync(WebSocketMessageContext context)
        {
            var segments = context.ToSegment();
            await _webSocket.SendAsync(segments, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendBinaryAsync(byte[] bytes)
        {
            // TODO Chunked
            var segments = new ArraySegment<byte>(bytes, 0, bytes.Count());
            await _webSocket.SendAsync(segments, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        internal void Close(string statusDescription)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription, CancellationToken.None);
        }
    }
}
