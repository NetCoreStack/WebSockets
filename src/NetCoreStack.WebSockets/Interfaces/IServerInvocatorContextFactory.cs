namespace NetCoreStack.WebSockets
{
    public interface IServerInvocatorContextFactory<TInvocator> where TInvocator : IServerWebSocketCommandInvocator
    {
        InvocatorContext CreateInvocatorContext();
    }
}