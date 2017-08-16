using System;
using Microsoft.Extensions.DependencyInjection;

namespace NetCoreStack.WebSockets
{
    public static class WebSocketReceiverExtensions
    {
        public static IWebSocketCommandInvocator GetInvocator(this WebSocketReceiverContext context, IServiceProvider serviceProvider)
        {
            if (context.InvocatorContext != null)
            {
                var instance = serviceProvider.GetService(context.InvocatorContext.Invocator);
                if (instance != null && instance is IWebSocketCommandInvocator)
                {
                    return (IWebSocketCommandInvocator)instance;
                }
            }

            return serviceProvider.GetService<IServerWebSocketCommandInvocator>();
        }
    }
}
