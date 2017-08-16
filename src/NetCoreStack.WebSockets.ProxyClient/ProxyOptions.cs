namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ProxyOptions<TInvocator> : SocketsOptions<TInvocator> where TInvocator : IClientWebSocketCommandInvocator
    {
        public string ConnectorName { get; set; }
        public string WebSocketHostAddress { get; set; }

        public ProxyOptions()
        {
            ConnectorName = "";
        }

        public override string ConnectorKey()
        {
            return $"{ConnectorName}|{WebSocketHostAddress}|{InvocatorTypeHashCode}";
        }
    }
}
