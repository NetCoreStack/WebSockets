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
        private readonly ILogger<ClientWebSocketConnector> _logger;

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
            _logger = _loggerFactory.CreateLogger<ClientWebSocketConnector>();
        }

        public abstract ClientInvocatorContext InvocatorContext { get; }

        private async Task<ClientWebSocketReceiver> TryConnectAsync(CancellationToken cancellationToken)
        {
            _webSocket = new ClientWebSocket();
            _webSocket.Options.SetRequestHeader(NCSConstants.ConnectorName, InvocatorContext.ConnectorName);
            try
            {
                await _webSocket.ConnectAsync(InvocatorContext.Uri, cancellationToken);
            }
            catch (Exception ex)
            {
                ProxyLogHelper.Log(_loggerFactory, InvocatorContext, "Error", ex);
                return null;
            }

            var receiverContext = new ClientWebSocketReceiverContext
            {
                Compressor = _compressor,
                InvocatorContext = InvocatorContext,
                LoggerFactory = _loggerFactory,
                WebSocket = _webSocket
            };

            var receiver = new ClientWebSocketReceiver(_serviceProvider, receiverContext, Close, (connectionId) => {
                _connectionId = connectionId;
            });

            return receiver;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            ClientWebSocketReceiver receiver = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("===TryConnectAsync to: {0}", InvocatorContext.Uri.ToString());
                receiver = await TryConnectAsync(cancellationToken);
                if (receiver != null && WebSocketState == WebSocketState.Open)
                {
                    break;
                }

                _logger.LogInformation("===Retry...");
                await Task.Delay(1000);
            }

            _logger.LogInformation("===WebSocketConnected to: {0}", InvocatorContext.Uri.ToString());

            if (InvocatorContext.OnConnectedAsync != null)
            {
                await InvocatorContext.OnConnectedAsync(_webSocket);
            }            

            await Task.WhenAll(receiver.ReceiveAsync(cancellationToken));
            
            // Disconnected
            if (_webSocket.CloseStatus.HasValue || _webSocket.State == WebSocketState.Aborted)
            {
                if (InvocatorContext.OnDisconnectedAsync != null)
                {
                    await InvocatorContext.OnDisconnectedAsync(_webSocket);
                }
                else
                {
                    await ConnectAsync(cancellationToken);
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
            if (context.Header.TryGetValue(NCSConstants.ConnectionId, out connectionId))
            {
                var id = connectionId as string;
                if (string.IsNullOrEmpty(id))
                {
                    throw new InvalidOperationException(nameof(connectionId));
                }
            }
            else
            {
                context.Header.Add(NCSConstants.ConnectionId, ConnectionId);
            }

            return context.ToSegment();
        }

        public async Task SendAsync(WebSocketMessageContext context)
        {
            var segments = CreateTextSegment(context);
            await _webSocket.SendAsync(segments, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendBinaryAsync(byte[] bytes)
        {
            var segments = new ArraySegment<byte>(bytes, 0, bytes.Count());
            await _webSocket.SendAsync(segments, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        internal void Close(ClientWebSocketReceiverContext context)
        {
            context.WebSocket.Abort();
        }

        internal void Close(string statusDescription)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription, CancellationToken.None);
        }
    }
}
