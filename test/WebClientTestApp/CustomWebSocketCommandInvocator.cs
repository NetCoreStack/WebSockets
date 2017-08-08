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
        private readonly ILogger _logger;
        private readonly InMemoryCacheProvider _cacheProvider;

        public CustomWebSocketCommandInvocator(IConnectionManager connectionManager, InMemoryCacheProvider cacheProvider,  ILogger<CustomWebSocketCommandInvocator> logger)
        {
            _connectionManager = connectionManager;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        private Task InternalMethodAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            await Task.CompletedTask;

            if (context.MessageType == WebSocketMessageType.Binary)
            {
                var stateDictionary = context.Header as Dictionary<string, object>;
                if (stateDictionary != null)
                {
                    object key = null;
                    if (stateDictionary.TryGetValue("Key", out key))
                    {
                        var keyStr = key.ToString();
                        var descriptor = CacheHelper.GetDescriptor(key.ToString());
                        try
                        {
                            var genericList = descriptor.Type.CreateElementTypeAsGenericList();
                            var value = context.Value;
                            _cacheProvider.SetObject(keyStr, value, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });

#if DEBUG
                            var length = context.Length;
                            var message = $"===Sandbox: {Environment.MachineName}===Key: {keyStr}===Length: {length}===";
                            _logger.LogDebug(message);
#endif
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            var length = context.Length;
                            var message = $"===Exception: {ex.Message}===Sandbox: {Environment.MachineName}===Key: {keyStr}===Length: {length}===";
                            _logger.LogError(message);
#endif
                        }
                    }
                }
            }
        }
    }
}
