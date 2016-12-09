using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace NetCoreStack.WebSockets.Internal
{
    public static class InvocatorRegistryHelper
    {
        public static void Register<T>(this IServiceCollection services, Action<T> setup) where T : SocketsOptions, new()
        {
            services.AddSingleton<InvocatorRegistry>();
            var connectorOptions = Activator.CreateInstance<T>();
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
