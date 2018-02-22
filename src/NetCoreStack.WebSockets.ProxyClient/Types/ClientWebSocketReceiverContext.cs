namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ClientWebSocketReceiverContext : WebSocketReceiverContextBase
    {
        public ClientInvocatorContext InvocatorContext { get; set; }
    }
}