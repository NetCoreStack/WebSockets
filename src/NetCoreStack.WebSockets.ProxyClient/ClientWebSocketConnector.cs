using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using System;
using System.Collections.Generic;
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
        private readonly IStreamCompressor _compressor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly InvocatorRegistry _invocatorRegistry;

        public string ConnectionId
        {
            get
            {
                return _connectionId;
            }
        }

        public WebSocketState WebSocketState
        {
            get
            {
                return _webSocket.State;
            }
        }

        public ProxyOptions Options { get; }

        public ClientWebSocketConnector(IOptions<ProxyOptions> options, 
            IStreamCompressor compressor,
            InvocatorRegistry invocatorRegistry,
            ILoggerFactory loggerFactory)
        {
            _compressor = compressor;
            _invocatorRegistry = invocatorRegistry;
            _loggerFactory = loggerFactory;
            Options = options.Value;
        }

        public async Task ConnectAsync()
        {
            try
            {
                var name = Options.ConnectorName;
                var uri = new Uri($"ws://{Options.WebSocketHostAddress}");
                _webSocket = new ClientWebSocket();
                _webSocket.Options.SetRequestHeader(SocketsConstants.ConnectorName, Options.ConnectorName);
                await _webSocket.ConnectAsync(uri, CancellationToken.None);
                var receiverContext = new WebSocketReceiverContext
                {
                    Compressor = _compressor,
                    InvocatorRegistry = _invocatorRegistry,
                    LoggerFactory = _loggerFactory,
                    Options = Options,
                    WebSocket = _webSocket
                };
                var receiver = new WebSocketReceiver(receiverContext, Close, (connectionId) => {
                    _connectionId = connectionId;
                });
                await receiver.ReceiveAsync();
            }
            catch (Exception ex)
            {
                var logger = _loggerFactory.CreateLogger<ClientWebSocketConnector>();
                logger.LogDebug(new EventId((int)WebSocketState.Aborted, nameof(WebSocketState.Aborted)),
                    ex,
                    "WebSocket connection end!",
                    Options);
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

        internal void Close(WebSocketReceiverContext context)
        {
            context.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                nameof(WebSocketReceiverContext), 
                CancellationToken.None);
        }

        internal void Close(string statusDescription)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                statusDescription, 
                CancellationToken.None);
        }
    }
}
