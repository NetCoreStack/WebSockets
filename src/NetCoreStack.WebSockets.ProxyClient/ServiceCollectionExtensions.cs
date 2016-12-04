using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProxyWebSockets(this IServiceCollection services, Action<ConnectorOptions> setup)
        {
            services.AddSingleton<IWebSocketConnector, ClientWebSocketConnector>();
            services.AddSingleton<InvocatorRegistry>();
            var connectorOptions = new ConnectorOptions { };
            setup?.Invoke(connectorOptions);
            if (connectorOptions.Invocators.Any())
            {
                foreach (var invocator in connectorOptions.Invocators)
                {
                    services.AddSingleton(invocator);
                }
            }

            services.AddSingleton(Options.Create(connectorOptions));
        }
    }
}
