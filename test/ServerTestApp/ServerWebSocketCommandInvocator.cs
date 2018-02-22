using Common.Libs;
using Microsoft.Extensions.Caching.Distributed;
using NetCoreStack.WebSockets;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ServerTestApp
{
    public class ServerWebSocketCommandInvocator : IServerWebSocketCommandInvocator
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IDistributedCache _cache;
        public ServerWebSocketCommandInvocator(IDistributedCache cache, IConnectionManager connectionManager)
        {
            _cache = cache;
            _connectionManager = connectionManager;
        }

        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            if (context.MessageType == WebSocketMessageType.Text
                && context.Command == WebSocketCommands.DataSend)
            {
                if (context.Header.TryGetValue(nameof(WebSocketHeaderNames.CacheRequest), out object cacheRequest))
                {
                    var connectionId = context.Value.ToString();
                    await _cache.SendCache(_connectionManager, connectionId);

                    return;
                }

                await _connectionManager.BroadcastAsync(context);
            }
        }
    }
}
