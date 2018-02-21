using NetCoreStack.WebSockets;
using System.Threading.Tasks;

namespace WebClientTestApp
{
    public class AgentsWebSocketCommandInvocator : IServerWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        public AgentsWebSocketCommandInvocator(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            if (context.Command == WebSocketCommands.Connect)
            {

            }

            // Sending incoming data from Backend zone to the Clients (Browsers)
            // await _connectionManager.BroadcastAsync(context);

            await _connectionManager.SendAsync(context.GetConnectionId(), context);
        }
    }
}
