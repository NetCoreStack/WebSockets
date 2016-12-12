using System.Collections.Generic;
using System.Net.WebSockets;

namespace NetCoreStack.WebSockets
{
    public class WebSocketMessageContext
    {
        public WebSocketMessageType MessageType { get; set; }

        public WebSocketCommands? Command { get; set; }

        public object Value { get; set; }

        public IDictionary<string, object> Header { get; set; }

        public int Length { get; set; }

        public string CommandText
        {
            get
            {
                if (Command.HasValue)
                {
                    return Command.ToString();
                }

                return string.Empty;
            }
        }

        public WebSocketMessageContext()
        {
            Header = new Dictionary<string, object>();
        }
    }
}
