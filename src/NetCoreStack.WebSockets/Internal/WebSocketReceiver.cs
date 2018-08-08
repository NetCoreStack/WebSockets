using Microsoft.Extensions.Logging;
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
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<WebSocketReceiver> _logger;

        public WebSocketReceiver(IServiceProvider serviceProvider, 
            WebSocketReceiverContext context, 
            Action<WebSocketReceiverContext> closeCallback,
            ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _closeCallback = closeCallback;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<WebSocketReceiver>();
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
                        var invocator = _context.GetInvocator(_serviceProvider);
                        if (invocator != null)
                        {
                            await invocator.InvokeAsync(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log(_context, "Error", ex);
                    }
                    result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
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
                        LogHelper.Log(_context, "Error", ex);
                    }
                    result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                }
            }

            await _context.WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken);
            _logger.LogInformation("WebSocketReceiver[Server] {0} has close status for connection: {1}", result.CloseStatus, _context.ConnectionId);
            _closeCallback?.Invoke(_context);
        }
    }
}
