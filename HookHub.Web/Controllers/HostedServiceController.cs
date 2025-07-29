using System.Threading.Tasks;

using HookHub.Core.Workers;
using HookHub.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace HookHub.Web.Controllers
{
    public class HostedServiceController : Controller
    {
        private TimedHostedService TimedHostedService { get; set; }
        public HostedServiceController(TimedHostedService timedHostedService)
        {
            TimedHostedService = timedHostedService;
        }

        public IActionResult Index()
        {
            return Json(TimedHostedService);
        }

        public async Task<IActionResult> Restart()
        {
            await TimedHostedService.Restart(System.Threading.CancellationToken.None);
            return Json(TimedHostedService);
        }

        public async Task<IActionResult> Start()
        {
            await TimedHostedService.Start();
            return Json(TimedHostedService);
        }

        public async Task<IActionResult> Stop()
        {
            await TimedHostedService.Stop(System.Threading.CancellationToken.None);
            return Json(TimedHostedService);
        }
    }
}
