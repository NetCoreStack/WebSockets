using System;
using System.Collections.Generic;

namespace NetCoreStack.WebSockets.ProxyClient
{
    internal static class InvocatorFactory
    {
        internal static IList<Type> Invocators { get; }

        static InvocatorFactory()
        {
            Invocators = new List<Type>();
        }

        internal static IList<IWebSocketConnector> GetConnectors(IServiceProvider serviceProvider)
        {
            IList<IWebSocketConnector> connectors = new List<IWebSocketConnector>();
            var connectorHandlerType = typeof(IWebSocketConnector<>);
            foreach (var item in Invocators)
            {
                Type[] args = { item };
                var genericConnectorHandler = connectorHandlerType.MakeGenericType(args);
                var instance = (IWebSocketConnector)serviceProvider.GetService(genericConnectorHandler);
                if (instance != null)
                {
                    connectors.Add(instance);
                }
            }

            return connectors;
        }
    }
}
