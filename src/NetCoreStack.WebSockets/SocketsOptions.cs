using System;

namespace NetCoreStack.WebSockets
{
    public abstract class SocketsOptions<TInvocator> where TInvocator : IWebSocketCommandInvocator
    {
        public Type Invocator { get; }

        public SocketsOptions()
        {
            Invocator = typeof(TInvocator);
        }
    }
}