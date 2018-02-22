using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using System;

namespace NetCoreStack.WebSockets
{
    public class ConnectionManagerOfT<TInvocator> : ConnectionManager, IConnectionManager<TInvocator> where TInvocator : IServerWebSocketCommandInvocator
    {
        private readonly IServerInvocatorContextFactory<TInvocator> _invocatorContextFactory;

        public override InvocatorContext InvocatorContext { get; }

        public ConnectionManagerOfT(IServiceProvider serviceProvider,
            IServerInvocatorContextFactory<TInvocator> invocatorContextFactory,
            IStreamCompressor compressor, 
            IHandshakeStateTransport initState, 
            IHeaderProvider headerProvider, ILoggerFactory loggerFactory) 
            : base(serviceProvider, compressor, initState, headerProvider, loggerFactory)
        {
            _invocatorContextFactory = invocatorContextFactory ?? throw new ArgumentNullException(nameof(invocatorContextFactory));
            InvocatorContext = _invocatorContextFactory.CreateInvocatorContext();
        }
    }
}