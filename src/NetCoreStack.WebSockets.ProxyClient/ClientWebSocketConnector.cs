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

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

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

        public abstract ClientInvocatorContext InvocatorContext { get; }

        private async Task<ClientWebSocketReceiver> TryConnectAsync(CancellationTokenSource cancellationTokenSource = null)
        {
            _webSocket = new ClientWebSocket();
            _webSocket.Options.SetRequestHeader(NCSConstants.ConnectorName, InvocatorContext.ConnectorName);
            try
            {
                CancellationToken token = cancellationTokenSource != null ? cancellationTokenSource.Token : CancellationToken.None;
                await _webSocket.ConnectAsync(InvocatorContext.Uri, token);
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

        public async Task ConnectAsync(CancellationTokenSource cancellationTokenSource = null)
        {
            if (cancellationTokenSource == null)
                cancellationTokenSource = new CancellationTokenSource();

            ClientWebSocketReceiver receiver = null;
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                receiver = await TryConnectAsync(cancellationTokenSource);
                if (receiver != null && WebSocketState == WebSocketState.Open)
                {
                    break;
                }
            }

            await Task.WhenAll(receiver.ReceiveAsync());
            
            // Handshake down try re-connect
            if (_webSocket.CloseStatus.HasValue)
            {
                await ConnectAsync(cancellationTokenSource);
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
            // _semaphoreSlim.Wait();
            await _webSocket.SendAsync(segments, WebSocketMessageType.Text, true, CancellationToken.None);
            // _semaphoreSlim.Release();
        }

        public async Task SendBinaryAsync(byte[] bytes)
        {
            var segments = new ArraySegment<byte>(bytes, 0, bytes.Count());
            // _semaphoreSlim.Wait();
            await _webSocket.SendAsync(segments, WebSocketMessageType.Binary, true, CancellationToken.None);
            // _semaphoreSlim.Release();
        }

        internal void Close(ClientWebSocketReceiverContext context)
        {
            context.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                nameof(ClientWebSocketReceiverContext), 
                CancellationToken.None);
        }

        internal void Close(string statusDescription)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription, CancellationToken.None);
        }
    }
}
