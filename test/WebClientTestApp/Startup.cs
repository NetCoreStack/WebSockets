using Common.Libs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System;

namespace WebClientTestApp
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

            // WebSockets for Browsers - User Agent ( javascript clients )
            services.AddNativeWebSockets<AgentsWebSocketCommandInvocator>();

            // Client WebSocket - Proxy connections
            var builder = services.AddProxyWebSockets();

            var connectorname = $"TestWebApp-{Environment.MachineName}";
            builder.Register<CustomWebSocketCommandInvocator>(connectorname, "localhost:7803");

            // Runtime context factory
            // builder.Register<AnotherEndpointWebSocketCommandInvocator, CustomInvocatorContextFactory>();

            services.AddControllersWithViews();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
