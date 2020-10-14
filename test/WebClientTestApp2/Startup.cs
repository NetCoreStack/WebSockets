using Common.Libs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System;

namespace WebClientTestApp2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<InMemoryCacheProvider>();

            var connectorname = $"TestApp2-{Environment.MachineName}";

            // WebSocket - Proxy
            var builder = services.AddProxyWebSockets()
                .Register<WebSocketCommandInvocator>(connectorname, "localhost:7803");

            builder.Register<WebSocketCommandInvocator>(connectorname, "localhost:5000");

            // WebSockets for Browsers - User Agent ( javascript clients )
            services.AddNativeWebSockets<AgentsWebSocketCommandInvocator>();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Proxy (Domain App) Client WebSocket - DMZ to API side connections
            app.UseProxyWebSockets();

            // User Agent WebSockets for Browsers
            app.UseNativeWebSockets();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
