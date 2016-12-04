using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext, 
            IConnectionManager manager, 
            ILoggerFactory loggerFactory)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await manager.Handshake(webSocket);
            }
            else
            {
                await next(httpContext);
            }
        }
    }
}
