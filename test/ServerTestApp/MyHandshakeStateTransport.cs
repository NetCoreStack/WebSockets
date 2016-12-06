using NetCoreStack.WebSockets.Common;
using System.Threading.Tasks;

namespace ServerTestApp
{
    public class MyHandshakeStateTransport : IHandshakeStateTransport
    {
        public Task<object> GetStateAsync()
        {
            return Task.FromResult<object>(0);
        }
    }

    public class CustomHandshakeStateTransport : IHandshakeStateTransport
    {
        public Task<object> GetStateAsync()
        {
            return Task.FromResult<object>(0);
        }
    }
}
