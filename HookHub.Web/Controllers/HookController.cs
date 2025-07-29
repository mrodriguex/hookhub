using System.Threading.Tasks;

using HookHub.Core.Workers;

using Microsoft.AspNetCore.Mvc;

namespace HookHub.Web.Controllers
{
    public class HookController : Controller
    {
        private Worker Worker { get; set; }
        public HookController(Worker worker)
        {
            Worker = worker;
        }

        public IActionResult Index()
        {
            return Json(Worker);
        }

        public async Task<IActionResult> Restart()
        {
            await Worker.Restart(System.Threading.CancellationToken.None);
            return Json(Worker);
        }

        public async Task<IActionResult> Start()
        {
            await Worker.Start();
            return Json(Worker);
        }

        public async Task<IActionResult> Stop()
        {
            await Worker.Stop(System.Threading.CancellationToken.None);
            return Json(Worker);
        }
    }
}
