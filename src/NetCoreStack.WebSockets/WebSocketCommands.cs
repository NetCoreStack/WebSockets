using System;

namespace NetCoreStack.WebSockets
{
    [Flags]
    public enum WebSocketCommands : byte
    {
        Connect = 1,
        DataSend = 2,
        Handshake = 4,
        ServerClose = 8,
        ServerAbort = 16,
        CacheReady = 32,
        CacheInvalidate = 64,
        All = 127
    }
}
