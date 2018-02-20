using Common.Libs;
using NetCoreStack.WebSockets.ProxyClient;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Tests
{
    public class AnotherEndpointWebSocketCommandInvocator : IClientWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly InMemoryCacheProvider _cacheProvider;

        public AnotherEndpointWebSocketCommandInvocator(IConnectionManager connectionManager, InMemoryCacheProvider cacheProvider)
        {
            _connectionManager = connectionManager;
            _cacheProvider = cacheProvider;
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
