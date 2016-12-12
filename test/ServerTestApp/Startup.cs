using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using Swashbuckle.Swagger.Model;
using System.IO;
using WebClientTestApp;

namespace ServerTestApp
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

            services.AddSingleton<ICompressor, DefaultCompressor>();
            services.AddTransient<IHandshakeStateTransport, MyHandshakeStateTransport>();

            // Add NetCoreStack Native Socket Services.
            services.AddNativeWebSockets(options => {
                options.RegisterInvocator<ServerWebSocketCommandInvocator>(WebSocketCommands.All);
            });
            
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseNativeWebSockets();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
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
