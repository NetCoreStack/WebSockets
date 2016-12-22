using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ConsoleAppProxyClient
{
    public class DataStreamingInvocator : IClientWebSocketCommandInvocator
    {
        public async Task InvokeAsync(WebSocketMessageContext context)
        {
            var values = await Task.Run(() => JsonConvert.SerializeObject(context));
            Console.WriteLine(values);
        }
    }
}
