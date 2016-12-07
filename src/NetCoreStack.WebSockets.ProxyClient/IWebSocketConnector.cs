using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public interface IWebSocketConnector
    {
        Task ConnectAsync();
        Task SendAsync(WebSocketMessageContext context);
        Task SendBinaryAsync(byte[] bytes);
    }
}
