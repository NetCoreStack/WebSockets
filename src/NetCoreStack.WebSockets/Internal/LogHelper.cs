using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace NetCoreStack.WebSockets.Internal
{
    internal class LogHelper
    {
        internal static void Log(WebSocketReceiverContext context, Exception ex)
        {
            var logger = context.LoggerFactory.CreateLogger<WebSocketReceiver>();
            logger.Log(context.Options.LogLevel,
                new EventId((int)WebSocketState.Aborted, nameof(WebSocketState.Aborted)),
                context.Options,
                ex,
                (msg, exception) => {

                    var values = new Dictionary<string, object>();
                    values.Add(nameof(ex.Message), ex.Message);
                    return JsonConvert.SerializeObject(values);
                });
        }
    }
}
