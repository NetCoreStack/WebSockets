using Microsoft.Extensions.DependencyInjection;
using NetCoreStack.WebSockets.Common;
using NetCoreStack.WebSockets.Internal;
using System;

namespace NetCoreStack.WebSockets
{
    public static class SocketServiceCollectionExtensions
    {
        public static void AddNativeWebSockets(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IHandshakeStateTransport, DefaultHandshakeStateTransport>();
            services.AddSingleton<IConnectionManager, ConnectionManager>();
        }
    }
}
