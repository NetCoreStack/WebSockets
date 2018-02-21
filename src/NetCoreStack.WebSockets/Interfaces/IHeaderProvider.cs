using System.Collections.Generic;

namespace NetCoreStack.WebSockets
{
    public interface IHeaderProvider
    {
        void Invoke(IDictionary<string, object> header);
    }
}
