using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using System.Net.WebSockets;

namespace NetCoreStack.WebSockets
{
    public class WebSocketReceiverContext
    {
        public string ConnectionId { get; set; }
        public WebSocket WebSocket { get; set; }
        public InvocatorContext InvocatorContext { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
        public IStreamCompressor Compressor { get; set; }
    }
}
