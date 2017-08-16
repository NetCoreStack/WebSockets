using Common.Libs;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System.Threading.Tasks;

namespace WebClientTestApp
{
    public class AnotherEndpointWebSocketCommandInvocator : IClientWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly InMemoryCacheProvider _cacheProvider;

        public AnotherEndpointWebSocketCommandInvocator(IConnectionManager connectionManager, InMemoryCacheProvider cacheProvider,  ILogger<CustomWebSocketCommandInvocator> logger)
        {
            _connectionManager = connectionManager;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        private Task InternalMethodAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            await Task.CompletedTask;
        }
    }
}
