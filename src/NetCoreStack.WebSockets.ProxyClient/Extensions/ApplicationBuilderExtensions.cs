using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        public static IApplicationBuilder UseProxyWebSockets(this IApplicationBuilder app, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfServiceNotRegistered(app.ApplicationServices);
            var appLifeTime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            IList<IWebSocketConnector> connectors = InvocatorFactory.GetConnectors(app.ApplicationServices);
            foreach (var connector in connectors)
            {
                InvocatorsHelper.EnsureHostPair(connector.InvocatorContext);
                appLifeTime.ApplicationStopping.Register(OnShutdown, connector);
                Task.Factory.StartNew(async () => await connector.ConnectAsync(cancellationToken), TaskCreationOptions.LongRunning);
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
