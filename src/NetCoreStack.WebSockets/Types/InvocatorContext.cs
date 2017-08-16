using System;

namespace NetCoreStack.WebSockets
{
    public class InvocatorContext
    {
        public string ConnectorName { get; set; }
        public string HostAddress { get; set; }
        public string ConnectorKey { get; set; }
        public Type Invocator { get; set; }
    }
}
