using System;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketCommandMap
    {
        public WebSocketCommands Command { get; }
        public Type Invocator { get; }

        public WebSocketCommandMap(WebSocketCommands command, Type invocator)
        {
            Command = command;
            Invocator = invocator;
        }
    }
}
