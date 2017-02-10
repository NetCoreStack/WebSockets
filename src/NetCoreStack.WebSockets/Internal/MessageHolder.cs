using System;

namespace NetCoreStack.WebSockets.Internal
{
    public class MessageHolder
    {
        public string ConnectionId { get; set; }
        public ArraySegment<byte> Segments { get; set; }
    }
}