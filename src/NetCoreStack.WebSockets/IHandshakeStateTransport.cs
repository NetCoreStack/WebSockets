using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Common
{
    public interface IHandshakeStateTransport
    {
        Task<object> GetStateAsync();
    }
}
