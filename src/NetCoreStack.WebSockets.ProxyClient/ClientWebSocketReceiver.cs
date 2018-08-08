using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Internal;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ClientWebSocketReceiver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ClientWebSocketReceiverContext _context;
        private readonly Action<ClientWebSocketReceiverContext> _closeCallback;
        private readonly Action<string> _handshakeCallback;
        private readonly ILogger<ClientWebSocketReceiver> _logger;

        public ClientWebSocketReceiver(IServiceProvider serviceProvider,
            ClientWebSocketReceiverContext context, 
            Action<ClientWebSocketReceiverContext> closeCallback, 
            Action<string> handshakeCallback = null)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _closeCallback = closeCallback;
            _handshakeCallback = handshakeCallback;
            _logger = context.LoggerFactory.CreateLogger<ClientWebSocketReceiver>();
        }

        public async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[NCSConstants.ChunkSize];
            var result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    byte[] inputs = null;
                    using (var ms = new MemoryStream())
                    {
                        while (!result.EndOfMessage)
                        {
                            await ms.WriteAsync(buffer, 0, result.Count);
                            result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        }

                        await ms.WriteAsync(buffer, 0, result.Count);
                        inputs = ms.ToArray();
                    }
                    try
                    {
                        var context = result.ToContext(inputs);
                        if (context.Command == WebSocketCommands.Handshake)
                        {
                            _context.ConnectionId = context.Value?.ToString();
                            _handshakeCallback?.Invoke(_context.ConnectionId);
                        }
                        var invocator = _context.GetInvocator(_serviceProvider);
                        if (invocator != null)
                        {
                            await invocator.InvokeAsync(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "{0} An error occurred for message type: {1}", NCSConstants.WarningSymbol, WebSocketMessageType.Text);
                    }

                    try
                    {
                        result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    }
                    catch (WebSocketException ex)
                    {
                        _logger.LogInformation("ClientWebSocketReceiver[Proxy] {0} has close status for connection: {1}", ex?.WebSocketErrorCode, _context.ConnectionId);
                        _closeCallback?.Invoke(_context);
                        return;
                    }
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    byte[] binaryResult = null;
                    using (var ms = new MemoryStream())
                    {
                        while (!result.EndOfMessage)
                        {
                            await ms.WriteAsync(buffer, 0, result.Count);
                            result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        }

                        await ms.WriteAsync(buffer, 0, result.Count);
                        binaryResult = ms.ToArray();
                    }
                    try
                    {
                        var context = await result.ToBinaryContextAsync(_context.Compressor, binaryResult);
                        var invocator = _context.GetInvocator(_serviceProvider);
                        if (invocator != null)
                        {
                            await invocator.InvokeAsync(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "ClientWebSocketReceiver {0} Invocator error occurred for message type: {1}", NCSConstants.WarningSymbol, WebSocketMessageType.Binary);
                    }
                    result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                }
            }

            await _context.WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken);
            _logger.LogInformation("ClientWebSocketReceiver[Proxy] {0} has close status for connection: {1}", result.CloseStatus, _context.ConnectionId);
            _closeCallback?.Invoke(_context);
        }
    }
}
