using NetCoreStack.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerTestApp
{
    public class MyHandshakeStateTransport : IHandshakeStateTransport
    {
        public Task<IDictionary<string, object>> GetStateAsync()
        {
            var dictionary = new Dictionary<string, object>();
            return Task.FromResult<IDictionary<string, object>>(dictionary);
        }
    }
}
