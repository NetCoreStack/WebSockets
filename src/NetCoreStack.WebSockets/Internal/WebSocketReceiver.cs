using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketReceiver
    {
        private readonly WebSocketReceiverContext _context;
        private readonly Action<WebSocketReceiverContext> _closeCallback;

        public WebSocketReceiver(WebSocketReceiverContext context, Action<WebSocketReceiverContext> closeCallback)
        {
            _context = context;
            _closeCallback = closeCallback;
        }

        private async Task InternalReceiveAsync()
        {
            var buffer = new byte[SocketsConstants.ChunkSize];
            var result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var context = result.ToContext(buffer);
                    if (context.Command == WebSocketCommands.Handshake)
                        _context.ConnectionId = context.Value?.ToString();

                    var _invocators = _context.InvocatorRegistry.GetInvocators(context, _context.Options);
                    foreach (var invoker in _invocators)
                    {
                        await invoker.InvokeAsync(context);
                    }
                    result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
                            result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
                    var context = await result.ToBinaryContextAsync(_context.Compressor, binaryResult);
                    var _invocators = _context.InvocatorRegistry.GetInvocators(context, _context.Options);
                    foreach (var invoker in _invocators)
                    {
                        await invoker.InvokeAsync(context);
                    }
                    result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            await _context.WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _closeCallback.Invoke(_context);
        }

        public async Task ReceiveAsync()
        {
            try
            {
                await InternalReceiveAsync();
            }
            catch (Exception ex)
            {
                var logger = _context.LoggerFactory.CreateLogger<ConnectionManager>();
                logger.LogDebug(new EventId((int)WebSocketState.Aborted, nameof(WebSocketState.Aborted)),
                    ex,
                    "WebSocket connection end!",
                    _context.Options);
            }
            finally
            {
                if (_context.WebSocket.State == WebSocketState.Aborted ||
                    _context.WebSocket.State == WebSocketState.Closed)
                {
                    _closeCallback?.Invoke(_context);
                }
            }
        }
    }
}
