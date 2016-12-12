using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public interface IHandshakeStateTransport
    {
        Task<IDictionary<string, object>> GetStateAsync();
    }
}
