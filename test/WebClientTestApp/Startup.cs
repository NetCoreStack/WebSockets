using Common.Libs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System;
using System.IO;

namespace WebClientTestApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<InMemoryCacheProvider>();

            var connectorname = $"TestWebApp-{Environment.MachineName}";

            // WebSockets for Browsers - User Agent ( javascript clients )
            services.AddNativeWebSockets<AgentsWebSocketCommandInvocator>();

            // Client WebSocket - Proxy connections
            services.AddProxyWebSockets()
                .Register<CustomWebSocketCommandInvocator>(connectorname, "localhost:7803");
                // .Register<AnotherEndpointWebSocketCommandInvocator>(connectorname, "localhost:5000"); // Another endpoint registration, host address must be unique

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

        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .AddCommandLine(args).Build();

            var host = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
