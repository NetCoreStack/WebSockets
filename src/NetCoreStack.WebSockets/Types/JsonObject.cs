using System.Collections.Generic;

namespace NetCoreStack.WebSockets
{
    public abstract class JsonObject
    {
        public JsonObject()
        {
        }

        public IDictionary<string, object> ToJson()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            Serialize(dictionary);
            return dictionary;
        }

        protected abstract void Serialize(IDictionary<string, object> value);
    }
}
