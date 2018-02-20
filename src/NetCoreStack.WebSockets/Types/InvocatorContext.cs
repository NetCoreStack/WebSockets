using System;

namespace NetCoreStack.WebSockets
{
    public class InvocatorContext
    {
        public string ConnectorName { get; }
        public WebSocketSupportedSchemes Scheme { get; }
        public string HostAddress { get; }
        public string UriPath { get; }
        public string Query { get; }
        public string ConnectorKey { get; }
        public Type Invocator { get; }
        public Uri Uri { get; }

        public InvocatorContext(Type invocator, string connectorName, string hostAddress,
            WebSocketSupportedSchemes scheme = WebSocketSupportedSchemes.WS,
            string uriPath = "",
            string query = "")
        {
            Invocator = invocator ?? throw new ArgumentNullException(nameof(invocator));
            ConnectorName = connectorName ?? throw new ArgumentNullException(nameof(connectorName));
            HostAddress = hostAddress ?? throw new ArgumentNullException(nameof(hostAddress));
            Scheme = scheme;
            UriPath = uriPath;
            Query = query;

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