using HookHub.Core.Hubs;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using HookHub.Core.Workers;

namespace HookHub.Hub
{
    /// <summary>
    /// Configures services and the HTTP request pipeline for the HookHub.Hub web application.
    /// Sets up SignalR hub, CORS, and other middleware for the hub service.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The application configuration.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The web hosting environment.
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The web hosting environment.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Configures services for the application.
        /// Adds controllers, SignalR, CORS, and the Worker background service.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddControllers();
            services.AddCors();
            services.AddOptions();
            services.AddRazorPages();
            services.AddSignalR();

            services.AddSingleton<Worker>();
            services.AddHostedService(sp => sp.GetRequiredService<Worker>());

            services.AddScoped<CoreHub>();

            services.Configure<HubOptions>(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = null;
            });

#if DEBUG
            services.AddControllersWithViews();
#endif
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// Sets up middleware, routing, and SignalR hub mapping.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        /// <param name="logger">The logger for logging configuration steps.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            // Now you can get ILogger here - DI is fully configured
            logger.LogInformation("Configuring application pipeline...");

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseRouting();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Hub}/{action=Index}");
                endpoints.MapRazorPages();
                endpoints.MapHub<CoreHub>("/HOOKHUBNET");
                endpoints.MapControllers();
            });

            logger.LogInformation("Application pipeline configured successfully");
        }
    }
}