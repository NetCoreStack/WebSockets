using Microsoft.AspNetCore.Hosting;
using NetCoreStack.WebSockets.ProxyClient;

namespace NetCoreStack.WebSockets.Tests
{
    public class CustomInvocatorContextFactory : IClientInvocatorContextFactory<AnotherEndpointWebSocketCommandInvocator>
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public CustomInvocatorContextFactory(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public ClientInvocatorContext CreateInvocatorContext()
        {
            return new ClientInvocatorContext(typeof(AnotherEndpointWebSocketCommandInvocator), 
                "TestMachineName", 
                "localhost:5003");
        }
    }
}
