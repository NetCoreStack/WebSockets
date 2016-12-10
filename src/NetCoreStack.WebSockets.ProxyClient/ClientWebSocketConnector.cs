using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using System;
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
        private readonly ProxyOptions _options;
        private readonly IStreamCompressor _compressor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly InvocatorRegistry _invocatorRegistry;

        public ClientWebSocketConnector(IOptions<ProxyOptions> options, 
            IStreamCompressor compressor,
            InvocatorRegistry invocatorRegistry,
            ILoggerFactory loggerFactory)
        {
            _options = options.Value;
            _compressor = compressor;
            _invocatorRegistry = invocatorRegistry;
            _loggerFactory = loggerFactory;
        }

        public async Task ConnectAsync()
        {
            try
            {
                var name = _options.ConnectorName;
                var uri = new Uri($"ws://{_options.WebSocketHostAddress}");
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(uri, CancellationToken.None);
                await WebSocketReceiver.Receive(_webSocket, _compressor, _invocatorRegistry, (SocketsOptions)_options);
            }
            catch (Exception ex)
            {
                var logger = _loggerFactory.CreateLogger<ClientWebSocketConnector>();
                logger.LogDebug(new EventId((int)WebSocketState.Aborted, nameof(WebSocketState.Aborted)),
                    ex,
                    "WebSocket connection end!",
                    _options);
            }
            finally
            {
                if (_webSocket != null)
                    _webSocket.Dispose();
            }
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
