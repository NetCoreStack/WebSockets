using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreStack.WebSockets.Interfaces;
using System;
using System.Net.WebSockets;
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
            IStreamCompressor compressor,
            InvocatorRegistry invocatorRegistry,
            IOptions<ServerSocketsOptions> options,
            ILoggerFactory loggerFactory)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await manager.ConnectAsync(webSocket);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
