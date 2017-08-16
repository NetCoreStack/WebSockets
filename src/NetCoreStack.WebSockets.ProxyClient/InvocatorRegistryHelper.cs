using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NetCoreStack.WebSockets.ProxyClient
{
    internal static class InvocatorRegistryHelper
    {
        public static void Register<TInvocator>(this IServiceCollection services, ProxyOptions<TInvocator> options) where TInvocator : IClientWebSocketCommandInvocator
        {
            var invocatorType = typeof(TInvocator);
            services.AddTransient(invocatorType);
            InvocatorFactory.Invocators.Add(invocatorType);
            services.AddSingleton(Options.Create(options));
        }
    }
}
