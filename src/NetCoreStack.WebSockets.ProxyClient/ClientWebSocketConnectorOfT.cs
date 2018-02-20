using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using System;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ClientWebSocketConnectorOfT<TInvocator> : ClientWebSocketConnector,
        IWebSocketConnector<TInvocator> where TInvocator : IClientWebSocketCommandInvocator
    {
        private readonly IClientInvocatorContextFactory<TInvocator> _invocatorContextFactory;

        public ProxyOptions<TInvocator> Options { get; }

        public override InvocatorContext GetInvocatorContext()
        {
            return _invocatorContextFactory.CreateInvocatorContext();
        }

        public ClientWebSocketConnectorOfT(IServiceProvider serviceProvider,
            IClientInvocatorContextFactory<TInvocator> invocatorContextFactory,
            IStreamCompressor compressor,
            ILoggerFactory loggerFactory)
            : base(serviceProvider, compressor, loggerFactory)
        {
            _invocatorContextFactory = invocatorContextFactory ?? throw new ArgumentNullException(nameof(invocatorContextFactory));
        }
    }
}
