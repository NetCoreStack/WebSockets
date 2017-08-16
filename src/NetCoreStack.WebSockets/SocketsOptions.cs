using System;

namespace NetCoreStack.WebSockets
{
    public abstract class SocketsOptions<TInvocator> where TInvocator : IWebSocketCommandInvocator
    {
        public Type Invocator { get; }

        public int InvocatorTypeHashCode { get; }

        public SocketsOptions()
        {
            Invocator = typeof(TInvocator);
            InvocatorTypeHashCode = Invocator.GetHashCode();
        }

        public abstract string ConnectorKey();
    }
}
