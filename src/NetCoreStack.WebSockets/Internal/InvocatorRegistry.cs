using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreStack.WebSockets.Internal
{
    public class InvocatorRegistry
    {
        private readonly IServiceProvider _serviceProvider;
        public IList<IWebSocketCommandInvocator> Invocators { get; }

        public InvocatorRegistry(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public List<IWebSocketCommandInvocator> GetInvocators<TOptions>(WebSocketMessageContext context, TOptions options) 
            where TOptions : SocketsOptions
        {
            List<IWebSocketCommandInvocator> invocators = new List<IWebSocketCommandInvocator>();
            if (context != null)
            {
                var commands = context.Command.GetUniqueFlags().OfType<WebSocketCommands>().ToList();
                foreach (var value in commands)
                {
                    List<Type> types = options.Map.Where(c => c.Command == value).Select(x => x.Invocator).ToList();
                    foreach (var type in types)
                    {
                        var service = _serviceProvider.GetService(type);
                        var invocator = service as IWebSocketCommandInvocator;

                        if (invocator != null && !invocators.Contains(invocator))
                        {
                            invocators.Add(invocator);
                        }
                    }
                }
            }
            return invocators;
        }
    }
}
