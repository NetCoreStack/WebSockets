using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public interface IWebSocketConnector
    {
        string ConnectionId { get; }
        WebSocketState WebSocketState { get; }
        ProxyOptions Options { get; }
        Task ConnectAsync(CancellationTokenSource cancellationTokenSource);
        Task SendAsync(WebSocketMessageContext context);
        Task SendBinaryAsync(byte[] bytes);
    }
}
