using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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
                string connectorName = string.Empty;
                StringValues headerValue = "";
                if (httpContext.Request.Headers.TryGetValue(SocketsConstants.ConnectorName, out headerValue))
                {
                    connectorName = headerValue.ToString();
                }
                if (string.IsNullOrEmpty(connectorName))
                {
                    if (httpContext.Request.Query.ContainsKey(SocketsConstants.ConnectorName))
                    {
                        connectorName = httpContext.Request.Query[SocketsConstants.ConnectorName];
                    }
                }

                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await manager.ConnectAsync(webSocket, connectorName);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
