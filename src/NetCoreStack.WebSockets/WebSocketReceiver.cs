using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public static class WebSocketReceiver
    {
        public static async Task Receive<TOptions>(WebSocket webSocket, 
            IStreamCompressor compressor,
            InvocatorRegistry invocatorRegistry, 
            TOptions options) 
            where TOptions : SocketsOptions, new()
        {
            var buffer = new byte[SocketsConstants.ChunkSize];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var context = result.ToContext(buffer);
                    var _invocators = invocatorRegistry.GetInvocators(context, options);
                    foreach (var invoker in _invocators)
                    {
                        await invoker.InvokeAsync(context);
                    }
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    byte[] binaryResult = null;
                    using (var ms = new MemoryStream())
                    {
                        while (!result.EndOfMessage)
                        {
                            if (!result.CloseStatus.HasValue)
                            {
                                await ms.WriteAsync(buffer, 0, result.Count);
                            }
                            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        }
                        if (result.EndOfMessage)
                        {
                            if (!result.CloseStatus.HasValue)
                            {
                                await ms.WriteAsync(buffer, 0, result.Count);
                            }
                        }
                        binaryResult = ms.ToArray();
                    }
                    var context = await result.ToBinaryContextAsync(compressor, binaryResult);
                    var _invocators = invocatorRegistry.GetInvocators(context, options);
                    foreach (var invoker in _invocators)
                    {
                        await invoker.InvokeAsync(context);
                    }
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
