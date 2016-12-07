using System.Net.WebSockets;

namespace NetCoreStack.WebSockets
{
    public class WebSocketMessageContext
    {
        public WebSocketMessageType MessageType { get; set; }
        public WebSocketCommands? Command { get; set; }
        public object Value { get; set; }
        public object State { get; set; }

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
    }
}
