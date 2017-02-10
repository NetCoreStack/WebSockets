using System;

namespace NetCoreStack.WebSockets.Internal
{
    public class MessageHolder
    {
        public ArraySegment<byte> Segments { get; set; }

        public DateTime KeepTime { get; set; }
    }
}