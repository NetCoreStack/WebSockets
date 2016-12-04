using NetCoreStack.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Server
{
    public interface IServerWebSocketCommandInvocator
    {
        Task InvokeAsync(WebSocketMessageContext context);
    }
}
