using HookHub.Core.Workers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.IO;

namespace HookHub.Hook.Win
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string nombreExe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string dirExe = Path.GetDirectoryName(nombreExe);
            Directory.SetCurrentDirectory(dirExe);

            return Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();
            })
            .ConfigureServices(
                (hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json");
            })
            .UseWindowsService();
        }
    }
}
