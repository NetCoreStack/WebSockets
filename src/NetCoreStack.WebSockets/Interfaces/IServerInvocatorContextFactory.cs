using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoreStack.WebSockets
{
    public interface IServerInvocatorContextFactory<TInvocator> where TInvocator : IServerWebSocketCommandInvocator
    {
        InvocatorContext CreateInvocatorContext();
    }
}