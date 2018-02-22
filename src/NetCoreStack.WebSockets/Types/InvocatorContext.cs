using System;

namespace NetCoreStack.WebSockets
{
    public class InvocatorContext
    {
        public Type Invocator { get; }

        public InvocatorContext(Type invocator)
        {
            Invocator = invocator ?? throw new ArgumentNullException(nameof(invocator));
        }
    }
}