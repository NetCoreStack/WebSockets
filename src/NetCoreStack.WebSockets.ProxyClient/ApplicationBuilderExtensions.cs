using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class ApplicationBuilderExtensions
    {
        private static void ThrowIfServiceNotRegistered(IServiceProvider applicationServices)
        {
            var service = applicationServices.GetService<ProxyClientMarkerService>();
            if (service == null)
                throw new InvalidOperationException(string.Format("Required services are not registered - are you missing a call to AddProxyWebSockets?"));
        }

        public static IApplicationBuilder UseProxyWebSockets(this IApplicationBuilder app, CancellationTokenSource cancellationTokenSource = null)
        {
            ThrowIfServiceNotRegistered(app.ApplicationServices);
            var appLifeTime = app.ApplicationServices.GetService<IApplicationLifetime>();
            IList<IWebSocketConnector> connectors = InvocatorFactory.GetConnectors(app.ApplicationServices);
            foreach (var connector in connectors)
            {
                InvocatorsHelper.EnsureHostPair(connector.GetInvocatorContext());
                appLifeTime.ApplicationStopping.Register(OnShutdown, connector);
                Task.Factory.StartNew(async () => await connector.ConnectAsync(cancellationTokenSource), TaskCreationOptions.LongRunning);
            }

            return app;
        }

        private static void OnShutdown(object state)
        {
            try
            {
                var connector = state as ClientWebSocketConnector;
                if (connector != null)
                {
                    connector.Close(nameof(OnShutdown));
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
