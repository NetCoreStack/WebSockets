using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public interface IWebSocketCommandInvocator
    {
        Task InvokeAsync(WebSocketMessageContext context);
    }
}