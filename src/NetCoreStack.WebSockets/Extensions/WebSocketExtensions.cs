using NetCoreStack.WebSockets.Interfaces;
using NetCoreStack.WebSockets.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var webSocketContext = new WebSocketMessageContext();
            bool compression = GZipHelper.IsGZipHeader(input);
            byte[] bytes = null;
            if (compression)
            {
                bytes = await compressor.DeCompressAsync(input);
            }
            else
            {
                bytes = input;
            }

            using (var ms = new MemoryStream(bytes))
            using (var sr = new StreamReader(ms))
            {
                var content = await sr.ReadToEndAsync();
                if (content != null)
                {
                    string[] parts = content.Split(new string[] { SocketsConstants.Splitter }, StringSplitOptions.None);
                    if (parts.Length != 2)
                    {
                        throw new InvalidOperationException($"Invalid binary data format! " +
                            $"Check the splitter pattern exist: \"{SocketsConstants.Splitter}\"");
                    }

                    webSocketContext.Value = parts.Last();

                    try
                    {
                        webSocketContext.State = JsonConvert.DeserializeObject<Dictionary<string, object>>(parts.First());
                    }
                    catch (Exception ex)
                    {
                        webSocketContext.State = new Dictionary<string, object>
                        {
                            ["Exception"] = ex.Message,
                            ["Unknown"] = "Unknown binary message"
                        };
                    }
                }

                webSocketContext.Length = input.Length;
                webSocketContext.MessageType = WebSocketMessageType.Binary;
                webSocketContext.Command = WebSocketCommands.DataSend;
            }

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
