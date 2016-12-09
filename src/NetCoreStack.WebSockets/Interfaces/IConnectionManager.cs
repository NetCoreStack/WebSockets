using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public interface IConnectionManager
    {
        Task BroadcastAsync(WebSocketMessageContext context);

        Task BroadcastBinaryAsync(byte[] bytes, JsonObject properties);

        Task SendAsync(string connectionId, WebSocketMessageContext context);

        Task SendBinaryAsync(string connectionId, byte[] bytes, JsonObject properties);

        Task SendAsync(string connectionId, WebSocketMessageContext context, WebSocket webSocket);

        void CloseConnection(string connectionId);
    }
}
