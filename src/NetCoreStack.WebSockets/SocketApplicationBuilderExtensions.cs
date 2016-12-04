using Microsoft.AspNetCore.Builder;
using NetCoreStack.WebSockets.Internal;
using System;

namespace NetCoreStack.WebSockets
{
    public static class SocketApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseNativeWebSockets(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseWebSockets();
            app.UseMiddleware<WebSocketMiddleware>();

            return app;
        }
    }
}
