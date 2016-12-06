using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Common
{
    public class DefaultHandshakeStateTransport : IHandshakeStateTransport
    {
        public Task<object> GetStateAsync()
        {
            return Task.FromResult<object>(0);
        }
    }
}
