using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;

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
            services.AddMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "RedisInstance";
            });
            
            services.AddTransient<IHandshakeStateTransport, MyHandshakeStateTransport>();

            // Add NetCoreStack Native Socket Services.
            services.AddNativeWebSockets<ServerWebSocketCommandInvocator>();

            services.AddOpenApiDocument();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            var appLifeTime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            app.UseNativeWebSockets(appLifeTime.ApplicationStopped);

            app.UseOpenApi(config => {
                config.PostProcess = (document, request) =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = $"Platform API";
                };
            }); // serve OpenAPI/Swagger documents

            app.UseSwaggerUi3(); // serve Swagger UI

            app.UseReDoc(); // serve ReDoc UI

            app.UseRouting();

            app.UseEndpoints(routes =>
            {
                routes.MapControllers();
            });
        }
    }
}
