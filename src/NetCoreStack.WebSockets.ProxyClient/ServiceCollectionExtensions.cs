using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetCoreStack.WebSockets.Common;
using System;
using System.Linq;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProxyWebSockets(this IServiceCollection services, Action<ConnectorOptions> setup)
        {
            services.AddTransient<IHandshakeStateTransport, DefaultHandshakeStateTransport>();
            services.AddSingleton<IWebSocketConnector, ClientWebSocketConnector>();
            services.AddSingleton<InvocatorRegistry>();
            var connectorOptions = new ConnectorOptions { };
            setup?.Invoke(connectorOptions);
            if (connectorOptions.Invocators.Any())
            {
                foreach (var invocator in connectorOptions.Invocators)
                {
                    services.AddTransient(invocator);
                }
            }

            services.AddSingleton(Options.Create(connectorOptions));
        }
    }
}
