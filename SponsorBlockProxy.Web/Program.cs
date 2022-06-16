using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SponsorBlockProxy.Audio.FP;

namespace SponsorBlockProxy.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var webHost = CreateHostBuilder(args).Build();

            using (var scope = webHost.Services.CreateScope())
            {
                var fpService = scope.ServiceProvider.GetRequiredService<FPService>();

                await fpService.StartupRegisterAll();
            }

            await webHost.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    if (hostingContext.HostingEnvironment.IsProduction())
                    {
                        var path = System.Environment.GetEnvironmentVariable("SBP_CONFIG_DIR");
                        
                        config.AddJsonFile(Path.Combine(path, "appsettings.json"), optional: false, reloadOnChange: true);
                        config.AddJsonFile(Path.Combine(path, "appsettings.Production.json"), optional: true, reloadOnChange: true);
                    }
                }).ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
