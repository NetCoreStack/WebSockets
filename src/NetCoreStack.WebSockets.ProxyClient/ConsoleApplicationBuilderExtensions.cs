using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient.Console
{
    public static class ConsoleApplicationBuilderExtensions
    {
        public static IServiceProvider UseProxyWebSocket(this IServiceProvider serviceProvider, CancellationTokenSource cancellationTokenSource = null)
        {
            IList<IWebSocketConnector> connectors = InvocatorFactory.GetConnectors(serviceProvider);
            foreach (var connector in connectors)
            {
                Task.Run(async () => await connector.ConnectAsync(cancellationTokenSource));
            }

            return serviceProvider;
        }
    }
}
