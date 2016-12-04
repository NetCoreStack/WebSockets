using NetCoreStack.WebSockets;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public interface IConnectionManager
    {
        Task BroadcastAsync(WebSocketMessageContext context);

        Task SendAsync(string connectionId, WebSocketMessageContext context);

        Task SendAsync(string connectionId, WebSocketMessageContext context, WebSocket webSocket);
        void CloseConnection(string connectionId);
    }
}
