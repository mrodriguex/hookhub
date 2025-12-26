using HookHub.Core.Hubs;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using HookHub.Core.Workers;

namespace HookHub.Hub
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        // Remove ILogger injection from constructor
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // If you need logging in ConfigureServices, get it from services
            // but wait until services are built
            
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