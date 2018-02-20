using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.ProxyClient;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace NetCoreStack.WebSockets.Tests
{
    public class ProxyBuilderTests
    {
        private readonly IServiceCollection _services;
        private readonly IApplicationBuilder _appBuilder;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _hostingEnvironment;

        protected IServiceProvider Services => _appBuilder.ApplicationServices;

        public ProxyBuilderTests()
        {
            _services = new ServiceCollection();
            _loggerFactory = new LoggerFactory();
            _hostingEnvironment = new HostingEnvironment
            {
                ApplicationName = typeof(ProxyBuilderTests).Namespace,
                ContentRootPath = Directory.GetCurrentDirectory(),
                EnvironmentName = EnvironmentName.Development
            };
            

            // WebSockets for Browsers - User Agent ( browser clients )
            _services.AddNativeWebSockets<AgentsWebSocketCommandInvocator>();

            // Client WebSocket - Proxy connections
            var builder = _services.AddProxyWebSockets();

            var connectorname = $"TestWebApp-{Environment.MachineName}";
            builder.Register<CustomWebSocketCommandInvocator>(connectorname, "localhost:7803");

            // localhost:5003
            builder.Register<AnotherEndpointWebSocketCommandInvocator, CustomInvocatorContextFactory>();

            var serviceStarting = new ManualResetEvent(false);
            var lifetimeStart = new ManualResetEvent(false);
            var lifetimeContinue = new ManualResetEvent(false);

            IApplicationLifetime applicationLifetime = new ApplicationLifetime(new Logger<ApplicationLifetime>(_loggerFactory));
            _services.AddSingleton(applicationLifetime);

            _services.AddSingleton(_hostingEnvironment);

            _appBuilder = new ApplicationBuilder(_services.BuildServiceProvider());
        }

        [Fact]
        public void CustomInvocatorRegistryTest()
        {
            var customWebSocketCommandInvocator = Services.GetService<IWebSocketConnector<CustomWebSocketCommandInvocator>>();
            Assert.IsType<ClientWebSocketConnectorOfT<CustomWebSocketCommandInvocator>>(customWebSocketCommandInvocator);

            var context = customWebSocketCommandInvocator.GetInvocatorContext();
            Assert.Equal($"TestWebApp-{Environment.MachineName}", context.ConnectorName);
            Assert.Equal("localhost:7803", context.HostAddress);

            _appBuilder.UseProxyWebSockets();
        }

        [Fact]
        public void AnotherEndpointWebSocketCommandInvocator()
        {
            var customWebSocketCommandInvocator = Services.GetService<IWebSocketConnector<AnotherEndpointWebSocketCommandInvocator>>();
            Assert.IsType<ClientWebSocketConnectorOfT<AnotherEndpointWebSocketCommandInvocator>>(customWebSocketCommandInvocator);

            var context = customWebSocketCommandInvocator.GetInvocatorContext();
            Assert.Equal("TestMachineName", context.ConnectorName);
            Assert.Equal("localhost:5003", context.HostAddress);

            _appBuilder.UseProxyWebSockets();
        }
    }
}
