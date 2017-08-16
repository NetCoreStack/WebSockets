using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using System;
using System.Linq;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class ServiceCollectionExtensions
    {
        private static void AddProxyWebSocketsInternal(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<ProxyClientMarkerService>();
            services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
            services.TryAdd(ServiceDescriptor.Singleton<IStreamCompressor, GZipStreamCompressor>());
            services.TryAdd(ServiceDescriptor.Transient<IHandshakeStateTransport, DefaultHandshakeStateTransport>());
        }

        public static ProxyWebSocketsBuilder AddProxyWebSockets(this IServiceCollection services)
        {
            services.AddSingleton<IWebSocketConnector>(_ => InvocatorFactory.GetConnectors(_).First());
            AddProxyWebSocketsInternal(services);
            return new ProxyWebSocketsBuilder(services);
        }
    }
}
