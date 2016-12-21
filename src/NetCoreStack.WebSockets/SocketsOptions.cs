using NetCoreStack.WebSockets.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NetCoreStack.WebSockets
{
    public class SocketsOptions
    {
        internal readonly List<WebSocketCommandMap> Map;

        public List<Type> Invocators { get; }

        public LogLevel LogLevel => LogLevel.Debug;

        public SocketsOptions()
        {
            Map = new List<WebSocketCommandMap>();
            Invocators = new List<Type>();
        }

        protected void Registry(Type invocatorType, WebSocketCommands commands)
        {
            Invocators.Add(invocatorType);
            var values = commands.GetUniqueFlags().OfType<WebSocketCommands>().ToList();
            foreach (var value in values)
            {
                var commandMap = new WebSocketCommandMap(value, invocatorType);
                Map.Add(commandMap);
            }
        }
    }
}
