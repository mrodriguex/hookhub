using System.Threading.Tasks;
using HookHub.Core.Workers;
using Microsoft.AspNetCore.Mvc;

namespace HookHub.Hook.Controllers
{
    /// <summary>
    /// API controller for managing the lifecycle of the hook service.
    /// Provides endpoints to start, stop, and restart the hook, and to get its status.
    /// </summary>
    public class HookController : Controller
    {
        /// <summary>
        /// The background worker managing the hook connection.
        /// </summary>
        private Worker _worker { get; set; }

        /// <summary>
        /// Constructor. Injects the Worker dependency.
        /// </summary>
        /// <param name="worker">The Worker instance managing the hook.</param>
        public HookController(Worker worker)
        {
            _worker = worker;
        }

        /// <summary>
        /// Gets the current status of the hook.
        /// </summary>
        /// <returns>JSON representation of the hook status.</returns>
        public IActionResult Index()
        {
            return Json(_worker.Hook);
        }

        /// <summary>
        /// Restarts the hook service.
        /// </summary>
        /// <returns>JSON representation of the hook status after restart.</returns>
        public async Task<IActionResult> Restart()
        {
            await _worker.Restart(System.Threading.CancellationToken.None);
            return Json(_worker.Hook);
        }

        /// <summary>
        /// Starts the hook service.
        /// </summary>
        /// <returns>JSON representation of the hook status after starting.</returns>
        public async Task<IActionResult> Start()
        {
            await _worker.Start();
            return Json(_worker.Hook);
        }

        /// <summary>
        /// Stops the hook service.
        /// </summary>
        /// <returns>JSON representation of the hook status after stopping.</returns>
        public async Task<IActionResult> Stop()
        {
            await _worker.Stop(System.Threading.CancellationToken.None);
            return Json(_worker.Hook);
        }
    }
}
