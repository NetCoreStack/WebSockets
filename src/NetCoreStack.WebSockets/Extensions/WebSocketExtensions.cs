using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task<WebSocketMessageContext> ToBinaryContextAsync(this WebSocketReceiveResult result,
            IStreamCompressor compressor,
            byte[] input)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var content = input.Split();
            byte[] header = content.Item1;
            byte[] body = content.Item2;

            var webSocketContext = new WebSocketMessageContext();
            bool isCompressed = GZipHelper.IsGZipBody(body);
            if (isCompressed)
            {
                body = await compressor.DeCompressAsync(body);
            }

            using (var ms = new MemoryStream(header))
            using (var sr = new StreamReader(ms))
            {
                var data = await sr.ReadToEndAsync();
                if (data != null)
                {
                    try
                    {
                        webSocketContext.Header = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                    }
                    catch (Exception ex)
                    {
                        webSocketContext.Header = new Dictionary<string, object>
                        {
                            ["Exception"] = ex.Message,
                            ["Unknown"] = "Unknown binary message!"
                        };
                    }
                }
            }

            using (var ms = new MemoryStream(body))
            using (var sr = new StreamReader(ms))
            {
                var data = await sr.ReadToEndAsync();
                webSocketContext.Value = data;
            }

            webSocketContext.Length = input.Length;
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

        public static string GetConnectionId(this WebSocketMessageContext webSocketContext)
        {
            if (webSocketContext == null)
            {
                throw new ArgumentNullException(nameof(webSocketContext));
            }

            object connectionId = null;
            if (webSocketContext.Header.TryGetValue(SocketsConstants.ConnectionId, out connectionId))
            {
                return connectionId.ToString();
            }

            throw new ArgumentOutOfRangeException(nameof(connectionId));
        }
    }
}
