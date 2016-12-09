using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, 
            IConnectionManager manager,
            InvocatorRegistry invocatorRegistry,
            IOptions<ServerSocketsOptions> options,
            IHandshakeStateTransport initState)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await manager.Handshake(webSocket, invocatorRegistry, options.Value, initState);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
