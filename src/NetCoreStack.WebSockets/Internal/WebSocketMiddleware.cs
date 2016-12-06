using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.Common;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketMiddleware
    {
        private readonly IHandshakeStateTransport _initState;
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next, IHandshakeStateTransport initState)
        {
            _next = next;
            _initState = initState;
        }

        public async Task Invoke(HttpContext httpContext, 
            IConnectionManager manager, 
            ILoggerFactory loggerFactory)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await manager.Handshake(webSocket, _initState);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
