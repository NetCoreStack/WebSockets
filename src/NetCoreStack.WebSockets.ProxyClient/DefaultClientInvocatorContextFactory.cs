using Microsoft.Extensions.Options;
using System;

namespace NetCoreStack.WebSockets.ProxyClient
{
    internal class DefaultClientInvocatorContextFactory<TInvocator> : IClientInvocatorContextFactory<TInvocator>
        where TInvocator : IClientWebSocketCommandInvocator
    {
        private readonly ProxyOptions<TInvocator> _proxyOptions;

        public DefaultClientInvocatorContextFactory(IOptions<ProxyOptions<TInvocator>> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _proxyOptions = options.Value;
        }

        public ClientInvocatorContext CreateInvocatorContext()
        {
            return new ClientInvocatorContext(_proxyOptions.Invocator, _proxyOptions.ConnectorName, _proxyOptions.WebSocketHostAddress);
        }
    }
}