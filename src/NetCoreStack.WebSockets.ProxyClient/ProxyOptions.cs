namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ProxyOptions : SocketsOptions
    {
        public string ConnectorName { get; set; }
        public string WebSocketHostAddress { get; set; }

        public ProxyOptions()
        {
            ConnectorName = "";
        }

        public void RegisterInvocator<TInvocator>(WebSocketCommands commands) where TInvocator : IClientWebSocketCommandInvocator
        {
            var invocatorType = typeof(TInvocator);
            Registry(invocatorType, commands);
        }
    }
}
