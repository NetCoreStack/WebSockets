using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public class DefaultHandshakeStateTransport : IHandshakeStateTransport
    {
        public Task<object> GetStateAsync()
        {
            return Task.FromResult<object>(0);
        }
    }
}
