using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketReceiver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly WebSocketReceiverContext _context;
        private readonly Action<WebSocketReceiverContext> _closeCallback;
        private readonly Action<string> _handshakeCallback;

        public WebSocketReceiver(IServiceProvider serviceProvider, 
            WebSocketReceiverContext context, 
            Action<WebSocketReceiverContext> closeCallback, 
            Action<string> handshakeCallback = null)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _closeCallback = closeCallback;
            _handshakeCallback = handshakeCallback;
        }

        private async Task InternalReceiveAsync()
        {
            var buffer = new byte[SocketsConstants.ChunkSize];
            var result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    try
                    {
                        var context = result.ToContext(buffer);
                        if (context.Command == WebSocketCommands.Handshake)
                        {
                            _context.ConnectionId = context.Value?.ToString();
                            _handshakeCallback?.Invoke(_context.ConnectionId);
                        }

                        var invocator = _context.GetInvocator(_serviceProvider);
                        invocator?.InvokeAsync(context);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log(_context, "Error", ex);
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
                    try
                    {
                        var context = await result.ToBinaryContextAsync(_context.Compressor, binaryResult);
                        var invocator = _context.GetInvocator(_serviceProvider);
                        invocator?.InvokeAsync(context);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log(_context, "Error", ex);
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
                LogHelper.Log(_context, "Error", ex);
            }
            finally
            {
                _closeCallback?.Invoke(_context);
            }
        }
    }
}
