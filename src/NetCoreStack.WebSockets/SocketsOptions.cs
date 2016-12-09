using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreStack.WebSockets
{
    public class SocketsOptions
    {
        internal readonly Dictionary<WebSocketCommands, Type> Map = 
            new Dictionary<WebSocketCommands, Type>();

        public List<Type> Invocators { get; }

        public SocketsOptions()
        {
            Invocators = new List<Type>();
        }

        protected void Registry(Type invocatorType, WebSocketCommands commands)
        {
            Invocators.Add(invocatorType);
            var values = commands.GetUniqueFlags().OfType<WebSocketCommands>().ToList();
            foreach (var value in values)
            {
                Map.Add(value, invocatorType);
            }
        }
    }
}
