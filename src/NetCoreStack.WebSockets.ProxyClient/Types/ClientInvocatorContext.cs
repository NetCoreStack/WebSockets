using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ClientInvocatorContext : InvocatorContext
    {
        public string ConnectorName { get; }
        public WebSocketSupportedSchemes Scheme { get; }
        public string HostAddress { get; }
        public string UriPath { get; }
        public string Query { get; }
        public string ConnectorKey { get; }
        public Uri Uri { get; }

        public Func<WebSocket, Task> OnConnectedAsync { get; set; }

        public Func<WebSocket, Task> OnDisconnectedAsync { get; set; }

        public ClientInvocatorContext(Type invocator, string connectorName, string hostAddress,
            WebSocketSupportedSchemes scheme = WebSocketSupportedSchemes.WS,
            string uriPath = "",
            string query = "",
            Func<WebSocket, Task> onConnectedAsync = null,
            Func<WebSocket, Task> onDisconnectedAsync = null)
            :base(invocator)
        {
            ConnectorName = connectorName ?? throw new ArgumentNullException(nameof(connectorName));
            HostAddress = hostAddress ?? throw new ArgumentNullException(nameof(hostAddress));
            Scheme = scheme;
            UriPath = uriPath;
            Query = query;

            OnConnectedAsync = onConnectedAsync;
            OnDisconnectedAsync = onDisconnectedAsync;

            var schemeStr = Scheme == WebSocketSupportedSchemes.WS ? "ws" : "wss";
            var uriBuilder = new UriBuilder(new Uri($"{schemeStr}://{HostAddress}"));
            if (!string.IsNullOrEmpty(UriPath))
            {
                uriBuilder.Path = UriPath;
            }

            if (!string.IsNullOrEmpty(Query))
            {
                uriBuilder.Query = Query;
            }

            Uri = uriBuilder.Uri;
            ConnectorKey = $"{ConnectorName}|{HostAddress}|{Invocator.GetHashCode()}";
        }
    }
}