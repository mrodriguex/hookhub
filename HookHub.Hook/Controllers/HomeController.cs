using HookHub.Core.Workers;
using Microsoft.AspNetCore.Mvc;

namespace HookHub.Hook.Controllers
{
    /// <summary>
    /// Controller for the home page of the hook web interface.
    /// Displays the current status and information of the hook.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The background worker managing the hook connection.
        /// </summary>
        private readonly Worker _worker;

        /// <summary>
        /// Constructor. Injects the Worker dependency.
        /// </summary>
        /// <param name="worker">The Worker instance managing the hook.</param>
        public HomeController(Worker worker)
        {
            _worker = worker;
        }

        /// <summary>
        /// Displays the home page with hook status information.
        /// </summary>
        /// <returns>The home view with the Worker model.</returns>
        public IActionResult Index()
        {
            return View(_worker);
        }
    }
}
