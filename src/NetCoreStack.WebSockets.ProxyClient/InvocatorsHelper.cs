using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class InvocatorsHelper
    {
        private static readonly InvocatorTypes _invocators = new InvocatorTypes(StringComparer.OrdinalIgnoreCase);

        private class InvocatorTypes : Dictionary<string, ConnectorHostPair>
        {
            public InvocatorTypes(IEqualityComparer<string> equalityComparer)
                :base(equalityComparer)
            {

            }
        }

        public static void EnsureHostPair(Type invocator, string connectorName, string hostAddress)
        {
            var connectorHostPair = new ConnectorHostPair(connectorName, hostAddress, invocator);
            var key = connectorHostPair.Key;

            if (_invocators.TryGetValue(key, out ConnectorHostPair value))
            {
                // throw new InvalidOperationException($"\"{connectorName}\" is already registered with same Host and Invocator");
                return;
            }

            _invocators.Add(key, connectorHostPair);
        }

        public static void EnsureHostPair(ClientInvocatorContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var invocator = context.Invocator;
            var connectorName = context.ConnectorName;
            var hostAddress = context.HostAddress;

            EnsureHostPair(invocator, connectorName, hostAddress);
        }

        public static List<Type> GetInvocators()
        {
            return _invocators.Select(p => p.Value.Invocator).ToList();
        }
    }
}
