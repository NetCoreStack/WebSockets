using Microsoft.Extensions.Options;
using System;
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

        public ClientWebSocketConnector(IOptions<ConnectorOptions> options, InvocatorRegistry invocatorRegistry)
        {
            _options = options.Value;
            _invocatorRegistry = invocatorRegistry;
        }

        public async Task InitializeAsync()
        {
            var uri = new Uri($"ws://{_options.WebSocketHostAddress}");
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(uri, CancellationToken.None);
            var buffer = new byte[1024 * 4];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var context = result.ToContext(buffer);
                if (context.Command == WebSocketCommands.Handshake)
                    _connectionId = context.Value.ToString();

                var _invocators = _invocatorRegistry.GetInvocators(context);
                _invocators.ForEach(async x => await x.InvokeAsync(context));
                result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async Task SendAsync(WebSocketMessageContext context)
        {
            var segments = context.ToSegment();
            await _webSocket.SendAsync(segments, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        internal void Close(string statusDescription)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription, CancellationToken.None);
        }
    }
}
