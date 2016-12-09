using Microsoft.Extensions.DependencyInjection;

namespace NetCoreStack.WebSockets
{
    public interface IWebSocketBuilder
    {
        IServiceCollection Services { get; }
    }
}
