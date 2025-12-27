using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HookHub.Hub
{
    /// <summary>
    /// Entry point for the HookHub.Hub web application.
    /// Configures and starts the ASP.NET Core web host.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method. Builds and runs the web host.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates and configures the IHostBuilder for the web application.
        /// Sets up logging and specifies the Startup class.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>An IHostBuilder configured for the web application.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                   .ConfigureLogging((hostingContext, logging) =>
                   {
                       // Configure logging from appsettings.json
                       logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                       logging.AddConsole();
                       logging.AddDebug();
                       logging.AddEventSourceLogger();
                   })
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                           // Specify Startup class for app configuration
                           webBuilder.UseStartup<Startup>();
                       });
        }
    }
}
