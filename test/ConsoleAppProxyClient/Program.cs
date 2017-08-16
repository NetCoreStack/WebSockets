using Microsoft.Extensions.DependencyInjection;
using NetCoreStack.WebSockets.ProxyClient;
using NetCoreStack.WebSockets.ProxyClient.Console;
using System;

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

            var connectorName = $"ConsoleApp-{Environment.MachineName}";

            services.AddProxyWebSockets()
                .Register<DataStreamingInvocator>(connectorName, "localhost:7803");

            _resolver = services.BuildServiceProvider();

            _resolver.UseProxyWebSocket();

            Console.ReadLine();
        }
    }
}
