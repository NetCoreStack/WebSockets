using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreStack.WebSockets.Interfaces;
using System;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ClientWebSocketConnectorOfInvocator<TInvocator> : ClientWebSocketConnector,
        IWebSocketConnector<TInvocator> where TInvocator : IClientWebSocketCommandInvocator
    {
        public ProxyOptions<TInvocator> Options { get; }

        protected override InvocatorContext CreateInvocatorContext()
        {
            var name = Options.ConnectorName;
            var connectorKey = Options.ConnectorKey();            
            var hostAddress = Options.WebSocketHostAddress;
            var invocatorType = Options.Invocator;
            return new InvocatorContext
            {
                ConnectorName = name,
                HostAddress = hostAddress,
                ConnectorKey = connectorKey,
                Invocator = invocatorType
            };
        }

        public ClientWebSocketConnectorOfInvocator(IServiceProvider serviceProvider,
            IOptions<ProxyOptions<TInvocator>> options,
            IStreamCompressor compressor,
            ILoggerFactory loggerFactory)
            : base(serviceProvider, compressor, loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Options = options.Value;
        }
    }
}
