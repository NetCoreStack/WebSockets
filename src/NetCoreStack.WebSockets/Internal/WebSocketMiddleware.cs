using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NetCoreStack.WebSockets.Interfaces;
using System;
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
                string connectionId = string.Empty;
                string connectorName = string.Empty;
                StringValues headerValue = "";
                if (httpContext.Request.Headers.TryGetValue(SocketsConstants.ConnectorName, out headerValue))
                {
                    connectorName = headerValue.ToString();
                }
                if (httpContext.Request.Headers.TryGetValue(SocketsConstants.ConnectionId, out headerValue))
                {
                    connectionId = headerValue.ToString();
                }

                if (string.IsNullOrEmpty(connectorName))
                {
                    if (httpContext.Request.Query.ContainsKey(SocketsConstants.ConnectorName))
                    {
                        connectorName = httpContext.Request.Query[SocketsConstants.ConnectorName];
                    }
                }
                if (string.IsNullOrEmpty(connectionId))
                {
                    if (httpContext.Request.Query.ContainsKey(SocketsConstants.ConnectionId))
                    {
                        connectionId = httpContext.Request.Query[SocketsConstants.ConnectionId];
                        Guid connectionIdGuid = Guid.Empty;
                        if (!Guid.TryParse(connectionId, out connectionIdGuid))
                        {
                            connectionId = string.Empty;
                        }
                    }
                }

                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                if (string.IsNullOrEmpty(connectionId))
                {
                    connectionId = Guid.NewGuid().ToString("N");
                }
                await manager.ConnectAsync(webSocket, connectionId: connectionId, connectorName: connectorName);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
