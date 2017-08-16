using System;

namespace NetCoreStack.WebSockets.ProxyClient
{
    internal class ConnectorHostPair : IEquatable<ConnectorHostPair>
    {
        public string ConnectorName { get; }
        public string HostAddress { get; }
        public Type Invocator { get; }

        public string Key { get; }

        public ConnectorHostPair(string connectorname, string hostAddress, Type invocator)
        {
            ConnectorName = connectorname ?? throw new ArgumentNullException(nameof(connectorname));
            HostAddress = hostAddress ?? throw new ArgumentNullException(nameof(hostAddress));
            Invocator = invocator ?? throw new ArgumentNullException(nameof(invocator));
            
            Key = $"{ConnectorName}|{HostAddress}|{Invocator.GetHashCode()}";
        }

        public bool Equals(ConnectorHostPair other)
        {
            return other != null && other.Key.Equals(Key, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConnectorHostPair);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public static bool operator ==(ConnectorHostPair left, ConnectorHostPair right)
        {
            if ((object)left == null)
            {
                return ((object)right == null);
            }
            else if ((object)right == null)
            {
                return ((object)left == null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(ConnectorHostPair left, ConnectorHostPair right)
        {
            return !(left == right);
        }
    }
}
