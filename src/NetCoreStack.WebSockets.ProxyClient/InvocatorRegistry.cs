using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class InvocatorRegistry
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConnectorOptions _options;
        public IList<IClientWebSocketCommandInvocator> Invocators { get; }

        public InvocatorRegistry(IOptions<ConnectorOptions> options, IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
        }

        public List<IClientWebSocketCommandInvocator> GetInvocators(WebSocketMessageContext context)
        {
            List<IClientWebSocketCommandInvocator> invocators = new List<IClientWebSocketCommandInvocator>();
            if (context != null)
            {
                var commands = context.Command.GetUniqueFlags().OfType<WebSocketCommands>().ToList();
                foreach (var value in commands)
                {
                    Type type;
                    if (_options._map.TryGetValue(value, out type))
                    {
                        var service = _serviceProvider.GetService(type);
                        var invocator = service as IClientWebSocketCommandInvocator;

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
