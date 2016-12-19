using Common.Libs;
using Microsoft.Extensions.Caching.Memory;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebClientTestApp2
{
    public class WebSocketCommandInvocator : IClientWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly InMemoryCacheProvider _cacheProvider;

        public WebSocketCommandInvocator(IConnectionManager connectionManager, InMemoryCacheProvider cacheProvider)
        {
            _connectionManager = connectionManager;
            _cacheProvider = cacheProvider;
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
                var header = context.Header as Dictionary<string, object>;
                if (header != null)
                {
                    object key = null;
                    if(header.TryGetValue(WebSocketHeaderNames.CacheItemKey, out key))
                    {
                        await Task.Run(() => _cacheProvider.SetObject(key.ToString(),
                             context.Value,
                             new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove }));
                    }
                }
                var length = context.Length;
                double size = (length / 1024f) / 1024f;
                context.Value = $"{size} MB <<binary>>";
            }

            // Sending incoming data from Backend zone to the Clients (Browsers)
            await _connectionManager.BroadcastAsync(context);
        }
    }
}
