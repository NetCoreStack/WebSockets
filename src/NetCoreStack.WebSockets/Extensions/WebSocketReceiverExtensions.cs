using System;

namespace NetCoreStack.WebSockets
{
    public static class WebSocketReceiverExtensions
    {
        public static IServerWebSocketCommandInvocator GetInvocator(this WebSocketReceiverContext context, IServiceProvider serviceProvider)
        {
            if (context.InvocatorContext != null)
            {
                var instance = serviceProvider.GetService(context.InvocatorContext.Invocator);
                if (instance != null && instance is IServerWebSocketCommandInvocator)
                {
                    return (IServerWebSocketCommandInvocator)instance;
                }
            }

            return null;
        }
    }
}
