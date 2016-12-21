using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.ProxyClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace NetCoreStack.WebSockets.Internal
{
    public static class ProxyLogHelper
    {
        public static void Log(ILoggerFactory loggerFactory, ProxyOptions options, Exception ex)
        {
            var logger = loggerFactory.CreateLogger<ClientWebSocketConnector>();
            logger.Log(options.LogLevel,
                new EventId((int)WebSocketState.Aborted, nameof(WebSocketState.Aborted)),
                options,
                ex,
                (msg, exception) => {

                    var values = new Dictionary<string, object>();
                    values.Add(nameof(ex.Message), ex.Message);
                    values.Add(nameof(options.ConnectorName), options.ConnectorName);
                    return JsonConvert.SerializeObject(values);
                });
        }
    }
}
