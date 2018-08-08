using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public interface IWebSocketConnector
    {
        string ConnectionId { get; }
        WebSocketState WebSocketState { get; }
        Task ConnectAsync(CancellationToken cancellationToken);
        Task SendAsync(WebSocketMessageContext context);
        Task SendBinaryAsync(byte[] bytes);
        ClientInvocatorContext InvocatorContext { get; }
    }

    public interface IWebSocketConnector<TInvocator> : IWebSocketConnector where TInvocator : IClientWebSocketCommandInvocator
    {
    }
}