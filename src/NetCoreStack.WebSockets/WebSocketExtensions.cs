using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;

namespace NetCoreStack.WebSockets
{
    public static class WebSocketExtensions
    {
        public static WebSocketMessageContext ToContext(this WebSocketReceiveResult result, byte[] buffer)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            
            var content = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var webSocketContext = JsonConvert.DeserializeObject<WebSocketMessageContext>(content);
            return webSocketContext;
        }

        public static ArraySegment<byte> ToSegment(this WebSocketMessageContext webSocketContext)
        {
            if (webSocketContext == null)
            {
                throw new ArgumentNullException(nameof(webSocketContext));
            }

            if (webSocketContext.Value == null)
            {
                throw new ArgumentNullException(nameof(webSocketContext.Value));
            }

            var content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(webSocketContext));
            return new ArraySegment<byte>(content, 0, content.Length);
        }
    }
}
