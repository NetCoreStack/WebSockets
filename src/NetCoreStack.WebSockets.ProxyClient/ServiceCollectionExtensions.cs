using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCoreStack.WebSockets.Interfaces;
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
            services.TryAdd(ServiceDescriptor.Singleton<IStreamCompressor, GZipStreamCompressor>());
            InvocatorRegistryHelper.Register(services, setup);
        }
    }
}
