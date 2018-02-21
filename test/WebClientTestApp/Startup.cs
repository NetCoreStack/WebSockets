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

            // Add framework services.
            services.AddMvc(options => {
                options.Filters.Add(new ClientExceptionFilterAttribute());
            });
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }    
    }
}
