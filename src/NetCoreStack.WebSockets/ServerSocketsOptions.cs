using System;

namespace NetCoreStack.WebSockets
{
    public class ServerSocketsOptions<TInvocator> : SocketsOptions<TInvocator> where TInvocator : IWebSocketCommandInvocator
    {
        public override string ConnectorKey()
        {
#if NET451
            return Environment.MachineName;
#else
            return Environment.GetEnvironmentVariable("COMPUTERNAME");
#endif
        }
    }
}
