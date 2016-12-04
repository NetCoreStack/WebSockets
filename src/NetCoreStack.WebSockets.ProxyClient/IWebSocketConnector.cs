using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public interface IWebSocketConnector
    {
        Task InitializeAsync();
        Task SendAsync(WebSocketMessageContext context);
    }
}
