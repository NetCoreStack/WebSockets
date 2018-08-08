using Microsoft.AspNetCore.Builder;
using NetCoreStack.WebSockets.Internal;
using System;
using System.Threading;

namespace NetCoreStack.WebSockets
{
    public static class SocketApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseNativeWebSockets(this IApplicationBuilder app, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseWebSockets();

            app.UseMiddleware<WebSocketMiddleware>(cancellationToken);

            return app;
        }
    }
}
