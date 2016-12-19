using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public interface IWebSocketConnector
    {
        string ConnectionId { get; }
        WebSocketState WebSocketState { get; }
        ProxyOptions Options { get; }
        Task ConnectAsync();
        Task SendAsync(WebSocketMessageContext context);
        Task SendBinaryAsync(byte[] bytes);
    }
}
