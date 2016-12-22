using Microsoft.Extensions.DependencyInjection;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using NetCoreStack.WebSockets.ProxyClient.Console;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ConsoleAppProxyClient
{
    public static class ApplicationVariables
    {
        public static int TaskCount = 0;
        public static int SocketReady = 0;
    }

    public class Program
    {
        private static IServiceProvider _resolver = null;

        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddProxyWebSockets(options =>
            {
                options.ConnectorName = $"ConsoleApp-{Environment.MachineName}";
                options.WebSocketHostAddress = "localhost:7803";
                options.RegisterInvocator<DataStreamingInvocator>(WebSocketCommands.All);
            });

            _resolver = services.BuildServiceProvider();

            _resolver.UseProxyWebSocket();

            Console.ReadLine();
        }
    }
}
