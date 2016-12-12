using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System.Collections.Generic;
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

        private Task InternalMethodAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            if (context.MessageType == WebSocketMessageType.Text)
            {

            }

            if (context.MessageType == WebSocketMessageType.Binary)
            {
                var state = context.State as Dictionary<string, object>;
                if (state != null)
                {
                    object value = null;
                    if(state.TryGetValue("Compressed", out value))
                    {

                    }
                }
                var length = context.Length;
                double size = (length / 1024f) / 1024f;
                context.Value = $"{size} MB <<binary>>";
            }

            // Sending incoming data from Backend zone to the Clients (Browsers)
            await _connectionManager.BroadcastAsync(context);
        }
    }
}
