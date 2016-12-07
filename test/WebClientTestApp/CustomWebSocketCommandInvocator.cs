using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.Internal;
using NetCoreStack.WebSockets.ProxyClient;
using System.Net.WebSockets;
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

        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            if (context.MessageType == WebSocketMessageType.Binary)
            {
                var state = context.State as SocketObject;
                if (state != null)
                {
                    var properties = state.ToJson();
                }
                var content = context.Value;
            }

            // Sending incoming data from Backend zone to the Clients (Browsers)
            await _connectionManager.BroadcastAsync(context);
        }
    }
}
