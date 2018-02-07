using System;
using System.Net.WebSockets;
using System.Threading;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketMessageDescriptor
    {
        public ArraySegment<byte> Segments { get; set; }
        public WebSocketMessageType MessageType { get; set; }
        public bool EndOfMessage { get; set; }
        public bool IsQueue { get; set; }
        public CancellationToken CancellationToken { get; }

        public WebSocketMessageDescriptor()
        {
            CancellationToken = CancellationToken.None;
        }
    }
}