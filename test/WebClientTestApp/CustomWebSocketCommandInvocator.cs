using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using NetCoreStack.WebSockets.Internal;
using System.Threading.Tasks;

namespace WebClientTestApp
{
    public class CustomWebSocketCommandInvocator : IClientWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        public CustomWebSocketCommandInvocator(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public Task InvokeAsync(WebSocketMessageContext context)
        {
            // Sending incoming data from Backend zone to the Clients (Browsers)
            _connectionManager.BroadcastAsync(context);
            return Task.CompletedTask;
        }
    }
}
