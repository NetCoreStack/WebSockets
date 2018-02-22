namespace NetCoreStack.WebSockets.ProxyClient
{
    public interface IClientInvocatorContextFactory<TInvocator> where TInvocator : IClientWebSocketCommandInvocator
    {
        ClientInvocatorContext CreateInvocatorContext();
    }
}