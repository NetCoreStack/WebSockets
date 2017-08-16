using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class ProxyWebSocketsBuilder
    {
        private IDictionary<string, ConnectorHostPair> _invocators;
        private readonly IServiceCollection _services;

        public ProxyWebSocketsBuilder(IServiceCollection services)
        {
            _invocators = new Dictionary<string, ConnectorHostPair>(StringComparer.OrdinalIgnoreCase);
            _services = services;
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
            var connectorHostPair = new ConnectorHostPair(connectorName, hostAddress, typeof(TInvocator));
            var key = connectorHostPair.Key;

            if(_invocators.TryGetValue(key, out ConnectorHostPair value))
            {
                throw new InvalidOperationException($"\"{connectorName}\" is already registered with same Host and Invocator");
            }

            _invocators.Add(key, connectorHostPair);
            _services.AddSingleton<IWebSocketConnector<TInvocator>, ClientWebSocketConnectorOfInvocator<TInvocator>>();
            var proxyOptions = new ProxyOptions<TInvocator>
            {
                ConnectorName = connectorName,
                WebSocketHostAddress = hostAddress
            };

            InvocatorRegistryHelper.Register<TInvocator>(_services, proxyOptions);
            return this;
        }        
    }
}
