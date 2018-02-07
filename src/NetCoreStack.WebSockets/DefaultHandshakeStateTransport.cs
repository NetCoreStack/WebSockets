using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public class DefaultHandshakeStateTransport : IHandshakeStateTransport
    {
        public Task<IDictionary<string, object>> GetStateAsync()
        {
            var dictionary = new Dictionary<string, object>();
            return Task.FromResult<IDictionary<string, object>>(dictionary);
        }
    }
}
