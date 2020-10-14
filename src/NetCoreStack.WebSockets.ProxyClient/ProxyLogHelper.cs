using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets.ProxyClient;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json;

namespace NetCoreStack.WebSockets.Internal
{
    public static class ProxyLogHelper
    {
        public static void Log(ILoggerFactory loggerFactory, ClientInvocatorContext context, string message, Exception ex = null)
        {
            LogLevel logLevel = LogLevel.Debug;
            if (ex != null)
                logLevel = LogLevel.Error;

            var logger = loggerFactory.CreateLogger<IWebSocketConnector>();
            var content = $"{message}=={ex?.Message}";

            logger.Log(logLevel,
                new EventId((int)WebSocketState.Aborted, nameof(WebSocketState.Aborted)),
                context,
                ex,
                (msg, exception) => {

                    var values = new Dictionary<string, object>();
                    values.Add("Message", content);
                    values.Add(nameof(context.HostAddress), context.HostAddress);
                    values.Add(nameof(context.ConnectorName), context.ConnectorName);
                    return JsonSerializer.Serialize(values);
                });
        }
    }
}
