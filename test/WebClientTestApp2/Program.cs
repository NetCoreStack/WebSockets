using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace WebClientTestApp2
{
    public class Program
    {
        public static Task Main(string[] args)
                            => BuildHost(args).RunAsync();

        public static IHost BuildHost(string[] args) => CreateHostBuilder(args).Build();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                .UseStartup<Startup>());
    }
}
