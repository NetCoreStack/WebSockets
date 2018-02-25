using Microsoft.Extensions.Caching.Distributed;
using NetCoreStack.WebSockets;
using System.Diagnostics;
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
            await Task.CompletedTask;
            Debug.WriteLine("Context length: {0}", context.Length);
        }
    }
}
