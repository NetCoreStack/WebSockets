using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseProxyWebSockets(this IApplicationBuilder app)
        {
            var appLifeTime = app.ApplicationServices.GetService<IApplicationLifetime>();
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(ClientWebSocketConnector));
            var webSocketConnector = app.ApplicationServices.GetService<IWebSocketConnector>();

            if (webSocketConnector != null && appLifeTime != null)
            {
                appLifeTime.ApplicationStopping.Register(OnShutdown, webSocketConnector);
                Task.Run(async () => await webSocketConnector.InitializeAsync());
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
