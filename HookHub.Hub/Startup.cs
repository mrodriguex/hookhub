using HookHub.Core.Hubs;
using HookHub.Core.Hooks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net;

namespace HookHub.Hub
{
    public class Startup : CoreHook
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            ILogger logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation("El servicio ha iniciado");

            ConnectClientAsync();
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

            services.AddSingleton<CoreHook>(this);
            services.AddSingleton<CoreHub>();

            services.Configure<HubOptions>(options =>
            {
                options.EnableDetailedErrors = true; 
                options.MaximumReceiveMessageSize = null;
            });
            var mvcBuilder = services.AddControllersWithViews();

#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation();
#endif

        }

        private async void ConnectClientAsync()
        {
            await Connect();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //app.UseWebSockets();
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
        }

    }
}
