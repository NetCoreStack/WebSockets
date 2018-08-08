using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using Swashbuckle.Swagger.Model;
using System;
using System.IO;

namespace ServerTestApp
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
            services.AddSwaggerGen(c =>
            {
                c.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "API V1",
                    TermsOfService = "None"
                });

                c.DescribeAllEnumsAsStrings();
            });

            services.AddMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "RedisInstance";
            });
            
            services.AddTransient<IHandshakeStateTransport, MyHandshakeStateTransport>();

            // Add NetCoreStack Native Socket Services.
            services.AddNativeWebSockets<ServerWebSocketCommandInvocator>();
            
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            var appLifeTime = app.ApplicationServices.GetService<IApplicationLifetime>();
            app.UseNativeWebSockets(appLifeTime.ApplicationStopped);

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
        }

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
