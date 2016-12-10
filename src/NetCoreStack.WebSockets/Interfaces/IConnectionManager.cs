using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public interface IConnectionManager
    {
        Task ConnectAsync(WebSocket webSocket);

        Task BroadcastAsync(WebSocketMessageContext context);

        Task BroadcastBinaryAsync(byte[] input, JsonObject properties);

        Task SendAsync(string connectionId, WebSocketMessageContext context);

        Task SendBinaryAsync(string connectionId, byte[] input, JsonObject properties);

        void CloseConnection(string connectionId);
    }
}
