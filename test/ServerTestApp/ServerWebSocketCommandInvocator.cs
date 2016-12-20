using NetCoreStack.WebSockets;
using System.Threading.Tasks;

namespace WebClientTestApp
{
    public class ServerWebSocketCommandInvocator : IServerWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        public ServerWebSocketCommandInvocator(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            var connection = context.GetConnectionId();

            // Sending incoming data from Backend zone to the Clients (Browsers)
            await _connectionManager.BroadcastAsync(context);
        }
    }
}
