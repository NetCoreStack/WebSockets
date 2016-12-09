using NetCoreStack.WebSockets.Internal;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;

namespace NetCoreStack.WebSockets
{
    public static class WebSocketExtensions
    {
        public static WebSocketMessageContext ToContext(this WebSocketReceiveResult result, byte[] values)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            
            var content = Encoding.UTF8.GetString(values, 0, result.Count);
            WebSocketMessageContext webSocketContext = new WebSocketMessageContext();
            try
            {
                webSocketContext = JsonConvert.DeserializeObject<WebSocketMessageContext>(content);
            }
            catch (Exception)
            {
                webSocketContext.Command = WebSocketCommands.DataSend;
                webSocketContext.Value = content;
                webSocketContext.MessageType = result.MessageType;
            }
            
            webSocketContext.Length = result.Count;
            return webSocketContext;
        }

        public static WebSocketMessageContext ToBinaryContext(this WebSocketReceiveResult result, byte[] values)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            
            var webSocketContext = new WebSocketMessageContext();
            var content = Encoding.UTF8.GetString(values);
            if (content != null)
            {
                string[] parts = content.Split(new string[] { SocketsConstants.Splitter }, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    throw new InvalidOperationException(nameof(parts));
                }

                webSocketContext.Value = parts.First();
                webSocketContext.State = JsonConvert.DeserializeObject<SocketObject>(parts.Last());
            }

            webSocketContext.Length = values.Length;
            webSocketContext.MessageType = WebSocketMessageType.Binary;
            webSocketContext.Command = WebSocketCommands.DataSend;
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
