using System;

namespace NetCoreStack.WebSockets
{
    [Flags]
    public enum WebSocketCommands : byte
    {
        DataSend = 1,
        Handshake = 2,
        ServerClose = 4,
        ServerAbort = 8,
        CacheReady = 16,
        All = 31
    }
}
