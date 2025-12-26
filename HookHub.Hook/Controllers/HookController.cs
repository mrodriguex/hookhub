using System.Threading.Tasks;
using HookHub.Core.Workers;
using Microsoft.AspNetCore.Mvc;

namespace HookHub.Hook.Controllers
{
    public class HookController : Controller
    {
        private Worker _worker { get; set; }
        public HookController(Worker worker)
        {
            _worker = worker;
        }

        public IActionResult Index()
        {
            return Json(_worker.Hook);
        }

        public async Task<IActionResult> Restart()
        {
            await _worker.Restart(System.Threading.CancellationToken.None);
            return Json(_worker.Hook);
        }

        public async Task<IActionResult> Start()
        {
            await _worker.Start();
            return Json(_worker.Hook);
        }

        public async Task<IActionResult> Stop()
        {
            await _worker.Stop(System.Threading.CancellationToken.None);
            return Json(_worker.Hook);
        }
    }
}
