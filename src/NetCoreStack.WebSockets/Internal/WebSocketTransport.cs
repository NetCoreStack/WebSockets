using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Internal
{
    public class WebSocketTransport
    {
        public WebSocket WebSocket { get; }
        public string ConnectionId { get; }

        public WebSocketTransport(WebSocket webSocket)
        {
            ConnectionId = Guid.NewGuid().ToString();
            WebSocket = webSocket;
        }

        public async Task Echo()
        {
            var buffer = new byte[1024 * 4];
            var result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                string content = "<<binary>>";
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    content = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (content.Equals("ServerClose"))
                    {
                        await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing from Server", CancellationToken.None);
                    }
                    else if (content.Equals("ServerAbort"))
                    {
                        WebSocket.Abort();
                    }
                }

                result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                LogFrame(result, buffer);
            }
            await WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private void LogFrame(WebSocketReceiveResult frame, byte[] buffer)
        {
            var close = frame.CloseStatus != null;
            string message;
            if (close)
            {
                message = $"Close: {frame.CloseStatus.Value} {frame.CloseStatusDescription}";
            }
            else
            {
                string content = "<<binary>>";
                if (frame.MessageType == WebSocketMessageType.Text)
                {
                    content = Encoding.UTF8.GetString(buffer, 0, frame.Count);
                }
                message = $"{frame.MessageType}: Len={frame.Count}, Fin={frame.EndOfMessage}: {content}";
            }
        }
    }
}
