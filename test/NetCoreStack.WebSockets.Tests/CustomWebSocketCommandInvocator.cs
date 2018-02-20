using Common.Libs;
using NetCoreStack.WebSockets.ProxyClient;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Tests
{
    public class CustomWebSocketCommandInvocator : IClientWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly InMemoryCacheProvider _cacheProvider;

        public CustomWebSocketCommandInvocator(IConnectionManager connectionManager, 
            InMemoryCacheProvider cacheProvider)
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

            if (context.MessageType == WebSocketMessageType.Text)
            {
                await _connectionManager.BroadcastAsync(context);
                return;
            }

            if (context.MessageType == WebSocketMessageType.Binary)
            {
               
            }
        }
    }
}
