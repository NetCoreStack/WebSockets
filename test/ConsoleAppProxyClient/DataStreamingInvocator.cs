using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleAppProxyClient
{
    public class DataStreamingInvocator : IClientWebSocketCommandInvocator
    {
        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            var values = await Task.Run(() => JsonSerializer.Serialize(context));
            Console.WriteLine(values);
        }
    }
}
