namespace NetCoreStack.WebSockets
{
    public class ServerSocketOptions<TInvocator> : SocketsOptions<TInvocator> where TInvocator : IServerWebSocketCommandInvocator
    {
    }
}