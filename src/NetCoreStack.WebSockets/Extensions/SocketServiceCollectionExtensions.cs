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
        public static void AddNativeWebSockets<TInvocator>(this IServiceCollection services)
            where TInvocator : IServerWebSocketCommandInvocator
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
            services.TryAdd(ServiceDescriptor.Singleton<IStreamCompressor, GZipStreamCompressor>());
            services.TryAdd(ServiceDescriptor.Transient<IHandshakeStateTransport, DefaultHandshakeStateTransport>());

            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddTransient(typeof(IServerWebSocketCommandInvocator), typeof(TInvocator));
        }
    }
}
