using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            services.TryAdd(ServiceDescriptor.Singleton<IHeaderProvider, DefaultHeaderProvider>());
            services.TryAdd(ServiceDescriptor.Singleton<IStreamCompressor, GZipStreamCompressor>());
            services.TryAdd(ServiceDescriptor.Transient<IHandshakeStateTransport, DefaultHandshakeStateTransport>());
            
            services.AddSingleton(Options.Create(new ServerSocketOptions<TInvocator>()));
            services.AddSingleton<IServerInvocatorContextFactory<TInvocator>, DefaultServerInvocatorContextFactory<TInvocator>>();
            services.AddSingleton<IConnectionManager<TInvocator>, ConnectionManagerOfT<TInvocator>>();

            services.AddSingleton(resolver => {
                return (IConnectionManager)resolver.GetService<IConnectionManager<TInvocator>>();
            });

            services.AddTransient(typeof(TInvocator));
        }
    }
}