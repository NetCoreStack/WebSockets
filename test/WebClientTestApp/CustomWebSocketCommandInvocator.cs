using Common.Libs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebClientTestApp
{
    public class CustomWebSocketCommandInvocator : IClientWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IWebSocketConnector<CustomWebSocketCommandInvocator> _webSocketConnector;
        private readonly ILogger _logger;
        private readonly InMemoryCacheProvider _cacheProvider;

        public CustomWebSocketCommandInvocator(IConnectionManager connectionManager,
            IWebSocketConnector<CustomWebSocketCommandInvocator> webSocketConnector,
            InMemoryCacheProvider cacheProvider, 
            ILogger<CustomWebSocketCommandInvocator> logger)
        {
            _connectionManager = connectionManager;
            _webSocketConnector = webSocketConnector;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        private Task InternalMethodAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            if (context.MessageType == WebSocketMessageType.Text)
            {
                
            }

            if (context.MessageType == WebSocketMessageType.Binary)
            {
                if (context.Header is Dictionary<string, object> stateDictionary)
                {
                    
                }
            }

            await _connectionManager.BroadcastAsync(context);
        }
    }
}
