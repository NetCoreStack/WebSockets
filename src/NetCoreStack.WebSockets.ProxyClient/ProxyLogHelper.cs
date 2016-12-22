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
        public static void Log(ILoggerFactory loggerFactory, ProxyOptions options, string message, Exception ex = null)
        {
            LogLevel logLevel = LogLevel.Debug;
            if (ex != null)
                logLevel = LogLevel.Error;

            var logger = loggerFactory.CreateLogger<ClientWebSocketConnector>();
            var content = $"{message}=={ex?.Message}";

            logger.Log(logLevel,
                new EventId((int)WebSocketState.Aborted, nameof(WebSocketState.Aborted)),
                options,
                ex,
                (msg, exception) => {

                    var values = new Dictionary<string, object>();
                    values.Add("Message", content);
                    values.Add(nameof(options.WebSocketHostAddress), options.WebSocketHostAddress);
                    values.Add(nameof(options.ConnectorName), options.ConnectorName);
                    return JsonConvert.SerializeObject(values);
                });
        }
    }
}
