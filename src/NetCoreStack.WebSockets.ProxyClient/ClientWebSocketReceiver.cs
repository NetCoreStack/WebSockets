using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public class ClientWebSocketReceiver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly WebSocketReceiverContext _context;
        private readonly Action<WebSocketReceiverContext> _closeCallback;
        private readonly Action<string> _handshakeCallback;
        private readonly ILogger<ClientWebSocketReceiver> _logger;

        public ClientWebSocketReceiver(IServiceProvider serviceProvider, 
            WebSocketReceiverContext context, 
            Action<WebSocketReceiverContext> closeCallback, 
            Action<string> handshakeCallback = null)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _closeCallback = closeCallback;
            _handshakeCallback = handshakeCallback;
            _logger = context.LoggerFactory.CreateLogger<ClientWebSocketReceiver>();
        }

        public async Task ReceiveAsync()
        {
            try
            {
                var buffer = new byte[NCSConstants.ChunkSize];
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
                            _logger.LogWarning(ex, "{0} Invocator error occurred for message type: {1}", NCSConstants.WarningSymbol, WebSocketMessageType.Text);
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
                            _logger.LogWarning(ex, "{0} Invocator error occurred for message type: {1}", NCSConstants.WarningSymbol, WebSocketMessageType.Binary);
                        }
                        result = await _context.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                }

                await _context.WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                _closeCallback?.Invoke(_context);
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    return;
                }

                var dictionary = new Dictionary<string, string>();
                dictionary.Add(nameof(_context.ConnectionId), _context.ConnectionId);

                if (_context.InvocatorContext != null)
                {
                    dictionary.Add(nameof(_context.InvocatorContext.ConnectorName), _context.InvocatorContext.ConnectorName);
                    dictionary.Add(nameof(_context.InvocatorContext.Uri), Convert.ToString(_context.InvocatorContext.Uri));
                }

                _logger.LogWarning(ex, "{0} receive exception: {1}", NCSConstants.WarningSymbol, JsonConvert.SerializeObject(dictionary));
            }
            finally
            {
                _closeCallback?.Invoke(_context);
            }
        }
    }
}
