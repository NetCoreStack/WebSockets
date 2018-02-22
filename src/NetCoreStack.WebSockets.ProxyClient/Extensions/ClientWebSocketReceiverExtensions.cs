using System;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class ClientWebSocketReceiverExtensions
    {
        public static IClientWebSocketCommandInvocator GetInvocator(this ClientWebSocketReceiverContext context, IServiceProvider serviceProvider)
        {
            if (context.InvocatorContext != null)
            {
                var instance = serviceProvider.GetService(context.InvocatorContext.Invocator);
                if (instance != null && instance is IClientWebSocketCommandInvocator)
                {
                    return (IClientWebSocketCommandInvocator)instance;
                }
            }

            return null;
        }
    }
}
