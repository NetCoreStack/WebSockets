using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public interface IHandshakeStateTransport
    {
        Task<object> GetStateAsync();
    }
}
