using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public interface IWebSocketConnector
    {
        string ConnectionId { get; }
        WebSocketState WebSocketState { get; }
        Task ConnectAsync(CancellationTokenSource cancellationTokenSource);
        Task SendAsync(WebSocketMessageContext context);
        Task SendBinaryAsync(byte[] bytes);
    }

    public interface IWebSocketConnector<THandler> : IWebSocketConnector where THandler : IClientWebSocketCommandInvocator
    {
        ProxyOptions<THandler> Options { get; }
    }
}
