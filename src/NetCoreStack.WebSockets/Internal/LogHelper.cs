using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json;

namespace NetCoreStack.WebSockets.Internal
{
    internal class LogHelper
    {
        internal static void Log(WebSocketReceiverContext context, string message, Exception ex = null)
        {
            LogLevel logLevel = LogLevel.Debug;
            if (ex != null)
                logLevel = LogLevel.Error;

            var logger = context.LoggerFactory.CreateLogger<WebSocketReceiver>();
            var content = $"{message}=={ex?.Message}";

            object state = new
            {
                context.ConnectionId
            };

            logger.Log(logLevel,
                new EventId((int)WebSocketState.Aborted, nameof(WebSocketState.Aborted)),
                state,
                ex,
                (msg, exception) => {
                    var values = new Dictionary<string, object>();
                    values.Add("Message", content);
                    return JsonSerializer.Serialize(values);
                });
        }
    }
}
