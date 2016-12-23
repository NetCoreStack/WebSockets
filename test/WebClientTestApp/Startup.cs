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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<InMemoryCacheProvider>();

            // Client WebSocket - DMZ to API side connections
            services.AddProxyWebSockets(options => {
                options.ConnectorName = $"TestApp-{Environment.MachineName}";
                options.WebSocketHostAddress = "localhost:7803";
                options.RegisterInvocator<CustomWebSocketCommandInvocator>(WebSocketCommands.All);
            });

            // WebSockets for Browsers - User Agent ( javascript clients )
            services.AddNativeWebSockets(options => {
                options.RegisterInvocator<AgentsWebSocketCommandInvocator>(WebSocketCommands.All);
            });

            // Add framework services.
            services.AddMvc(options => {
                options.Filters.Add(new ClientExceptionFilterAttribute());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
