using Microsoft.Extensions.DependencyInjection;

namespace NetCoreStack.WebSockets
{
    public class WebSocketBuilder : IWebSocketBuilder
    {
        public WebSocketBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
