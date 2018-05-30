using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ProxyWebSocketsBuilder
    {
        private readonly IServiceCollection _services;

        public ProxyWebSocketsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        private void RegisterInternal<TInvocator>() 
            where TInvocator : IClientWebSocketCommandInvocator
        {
            var invocatorType = typeof(TInvocator);
            InvocatorFactory.Invocators.Add(invocatorType);
            _services.AddTransient(invocatorType);
            _services.AddSingleton<IWebSocketConnector<TInvocator>, ClientWebSocketConnectorOfT<TInvocator>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInvocator"></typeparam>
        /// <param name="connectorName"></param>
        /// <param name="hostAddress">Unique host address</param>
        /// <returns></returns>
        public ProxyWebSocketsBuilder Register<TInvocator>(string connectorName, string hostAddress) 
            where TInvocator : IClientWebSocketCommandInvocator
        {
            var invocatorType = typeof(TInvocator);
            InvocatorsHelper.EnsureHostPair(invocatorType, connectorName, hostAddress);

            RegisterInternal<TInvocator>();

            var proxyOptions = new ProxyOptions<TInvocator>
            {
                ConnectorName = connectorName,
                WebSocketHostAddress = hostAddress
            };

            _services.AddSingleton(Options.Create(proxyOptions));
            _services.AddSingleton<IClientInvocatorContextFactory<TInvocator>, DefaultClientInvocatorContextFactory<TInvocator>>();
            return this;
        }   

        public ProxyWebSocketsBuilder Register<TInvocator, TContextFactory>() 
            where TInvocator : IClientWebSocketCommandInvocator
            where TContextFactory : IClientInvocatorContextFactory<TInvocator>
        {

            RegisterInternal<TInvocator>();

            _services.AddSingleton(typeof(IClientInvocatorContextFactory<TInvocator>), typeof(TContextFactory));
            
            return this;
        }
    }
}