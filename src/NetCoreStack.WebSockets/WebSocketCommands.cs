using System;

namespace NetCoreStack.WebSockets
{
    [Flags]
    public enum WebSocketCommands : byte
    {
        Connect = 1,
        DataSend = 2,
        Handshake = 4,
        All = 7
    }
}
