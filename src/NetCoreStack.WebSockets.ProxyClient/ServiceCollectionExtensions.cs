using Microsoft.Extensions.DependencyInjection;
using NetCoreStack.WebSockets.Internal;
using System;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProxyWebSockets(this IServiceCollection services, Action<ProxyOptions> setup)
        {
            services.AddTransient<IHandshakeStateTransport, DefaultHandshakeStateTransport>();
            services.AddSingleton<IWebSocketConnector, ClientWebSocketConnector>();
            InvocatorRegistryHelper.Register(services, setup);
        }
    }
}
