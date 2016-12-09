namespace NetCoreStack.WebSockets
{
    public class ServerSocketsOptions : SocketsOptions
    {
        public void RegisterInvocator<TInvocator>(WebSocketCommands commands) where TInvocator : IServerWebSocketCommandInvocator
        {
            var invocatorType = typeof(TInvocator);
            Registry(invocatorType, commands);
        }
    }
}
