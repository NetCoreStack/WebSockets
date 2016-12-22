using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using System;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProxyWebSockets(this IServiceCollection services, Action<ProxyOptions> setup)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
            services.TryAdd(ServiceDescriptor.Singleton<IStreamCompressor, GZipStreamCompressor>());

            services.AddTransient<IHandshakeStateTransport, DefaultHandshakeStateTransport>();
            services.AddSingleton<IWebSocketConnector, ClientWebSocketConnector>();
            InvocatorRegistryHelper.Register(services, setup);
        }
    }
}
