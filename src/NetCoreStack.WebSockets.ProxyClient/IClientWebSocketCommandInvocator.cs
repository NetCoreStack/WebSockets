using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public interface IClientWebSocketCommandInvocator
    {
        Task InvokeAsync(WebSocketMessageContext context);
    }
}