using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ConnectorOptions
    {
        internal readonly Dictionary<WebSocketCommands, Type> _map = 
            new Dictionary<WebSocketCommands, Type>();

        internal List<Type> Invocators { get; }

        public ConnectorOptions()
        {
            Invocators = new List<Type>();
        }

        public string WebSocketHostAddress { get; set; }
        public void RegisterInvocator<TInvocator>(WebSocketCommands commands) 
            where TInvocator : IClientWebSocketCommandInvocator
        {
            Invocators.Add(typeof(TInvocator));
            var values = commands.GetUniqueFlags().OfType<WebSocketCommands>().ToList();
            foreach (var value in values)
            {
                _map.Add(value, typeof(TInvocator));
            }
        }
    }
}
