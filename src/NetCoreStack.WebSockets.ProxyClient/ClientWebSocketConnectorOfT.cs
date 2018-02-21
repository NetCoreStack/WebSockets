using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using System;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ClientWebSocketConnectorOfT<TInvocator> : ClientWebSocketConnector,
        IWebSocketConnector<TInvocator> where TInvocator : IClientWebSocketCommandInvocator
    {
        private readonly IClientInvocatorContextFactory<TInvocator> _invocatorContextFactory;

        public override InvocatorContext InvocatorContext { get; }

        public ClientWebSocketConnectorOfT(IServiceProvider serviceProvider,
            IClientInvocatorContextFactory<TInvocator> invocatorContextFactory,
            IStreamCompressor compressor,
            ILoggerFactory loggerFactory)
            : base(serviceProvider, compressor, loggerFactory)
        {
            _invocatorContextFactory = invocatorContextFactory ?? throw new ArgumentNullException(nameof(invocatorContextFactory));
            InvocatorContext = _invocatorContextFactory.CreateInvocatorContext();
        }
    }
}