using System;
using System.Collections.Generic;

namespace NetCoreStack.WebSockets
{
    public class SocketObject : JsonObject
    {
        public string Key { get; set; }

        public object Value { get; set; }

        protected override void Serialize(IDictionary<string, object> value)
        {
            value.Add(nameof(Key), Key);
            value.Add(nameof(Value), Value);
        }
    }
}
