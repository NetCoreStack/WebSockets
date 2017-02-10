using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using System;

namespace NetCoreStack.WebSockets
{
    public static class SocketServiceCollectionExtensions
    {
        public static void AddNativeWebSockets(this IServiceCollection services, Action<ServerSocketsOptions> setup)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            services.AddSingleton<TransportLifetimeManager>();
            services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
            services.TryAdd(ServiceDescriptor.Singleton<IStreamCompressor, GZipStreamCompressor>());
            services.TryAdd(ServiceDescriptor.Transient<IHandshakeStateTransport, DefaultHandshakeStateTransport>());
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            InvocatorRegistryHelper.Register(services, setup);
        }
    }
}
