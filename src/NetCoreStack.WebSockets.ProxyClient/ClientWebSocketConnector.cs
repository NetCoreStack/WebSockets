using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public abstract class ClientWebSocketConnector : IWebSocketConnector
    {
        private string _connectionId;
        private ClientWebSocket _webSocket;
        private readonly IServiceProvider _serviceProvider;
        private readonly IStreamCompressor _compressor;
        private readonly ILoggerFactory _loggerFactory;

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
                if (_webSocket == null)
                    throw new InvalidOperationException("Make sure async instantiation completed and try again Connect!");

                return _webSocket.State;
            }
        }

        public ClientWebSocketConnector(IServiceProvider serviceProvider, 
            IStreamCompressor compressor, 
            ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _compressor = compressor;
            _loggerFactory = loggerFactory;
        }

        protected abstract InvocatorContext CreateInvocatorContext();

        private async Task TryConnectAsync(CancellationTokenSource cancellationTokenSource = null)
        {
            var invocatorContext = CreateInvocatorContext();
            var uri = new Uri($"ws://{invocatorContext.HostAddress}");
            _webSocket = new ClientWebSocket();
            _webSocket.Options.SetRequestHeader(SocketsConstants.ConnectorName, invocatorContext.ConnectorName);
            try
            {
                CancellationToken token = cancellationTokenSource != null ? cancellationTokenSource.Token : CancellationToken.None;
                await _webSocket.ConnectAsync(uri, token);
            }
            catch (Exception ex)
            {
                ProxyLogHelper.Log(_loggerFactory, invocatorContext, "Error", ex);
                return;
            }

            var receiverContext = new WebSocketReceiverContext
            {
                Compressor = _compressor,
                InvocatorContext = invocatorContext,
                LoggerFactory = _loggerFactory,
                WebSocket = _webSocket
            };
            var receiver = new WebSocketReceiver(_serviceProvider, receiverContext, Close, (connectionId) => {
                _connectionId = connectionId;
            });
            await receiver.ReceiveAsync();
        }

        public async Task ConnectAsync(CancellationTokenSource cancellationTokenSource = null)
        {
            if (cancellationTokenSource == null)
                cancellationTokenSource = new CancellationTokenSource();

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await TryConnectAsync(cancellationTokenSource);

                if (WebSocketState == WebSocketState.Open)
                {
                    break;
                }
            }
        }

        private ArraySegment<byte> CreateTextSegment(WebSocketMessageContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            object connectionId = string.Empty;
            if (context.Header.TryGetValue(SocketsConstants.ConnectionId, out connectionId))
            {
                var id = connectionId as string;
                if (string.IsNullOrEmpty(id))
                    throw new InvalidOperationException(nameof(connectionId));
            }
            else
                context.Header.Add(SocketsConstants.ConnectionId, ConnectionId);

            return context.ToSegment();
        }

        public async Task SendAsync(WebSocketMessageContext context)
        {
            var segments = CreateTextSegment(context);
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
