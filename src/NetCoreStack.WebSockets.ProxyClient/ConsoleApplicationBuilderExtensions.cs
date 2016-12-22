using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient.Console
{
    public static class ConsoleApplicationBuilderExtensions
    {
        public static IServiceProvider UseProxyWebSocket(this IServiceProvider services)
        {
            var loggerFactory = services.GetService<ILoggerFactory>();
            if (loggerFactory == null)
                loggerFactory = new LoggerFactory();

            var logger = loggerFactory.CreateLogger(nameof(ClientWebSocketConnector));
            var webSocketConnector = services.GetService<IWebSocketConnector>();
            if (webSocketConnector == null)
                throw new ArgumentNullException($"{nameof(webSocketConnector)} please try AddProxyWebSockets");

            Task.Run(async () => await webSocketConnector.ConnectAsync());

            return services;
        }
    }
}
