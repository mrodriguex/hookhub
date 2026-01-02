using HookHub.Core.Hubs;
using HookHub.Core.Workers;
using HookHub.Hub.Models;

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HookHub.Hub.Controllers
{
    /// <summary>
    /// Controller for the main pages of the hub web interface.
    /// Provides actions for home, about, contact, privacy, and error pages.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private CoreHookInfoModel _netClientInfo;

        /// <summary>
        /// The SignalR hub managing client connections.
        /// </summary>
        public CoreHub CoreHub { get; private set; }

        /// <summary>
        /// The background worker managing the hub.
        /// </summary>
        public Worker Worker { get; set; }

        /// <summary>
        /// Gets or sets the core hook information model.
        /// Lazily initializes from the Worker's hook.
        /// </summary>
        public CoreHookInfoModel CoreHookInfo
        {
            get
            {
                _netClientInfo ??= new CoreHookInfoModel(Worker.Hook);
                return (_netClientInfo);
            }
            set { _netClientInfo = value; }
        }

        /// <summary>
        /// Constructor. Injects the CoreHub and Worker dependencies.
        /// </summary>
        /// <param name="coreHub">The SignalR hub instance.</param>
        /// <param name="worker">The Worker instance managing the hub.</param>
        public HomeController(CoreHub coreHub, Worker worker)
        {
            Worker = worker;
            CoreHub = coreHub;
        }

          /// <summary>
        /// Displays the hub index page with hook information.
        /// </summary>
        /// <returns>The hub view with hook info model.</returns>
        [HttpGet("{Action}")]
        public IActionResult Index()
        {
            ViewData["Title"] = " - Hub Status";
            return View(CoreHookInfo);
        }

        /// <summary>
        /// Displays the about page.
        /// </summary>
        [HttpGet("About")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        /// <summary>
        /// Displays the contact page.
        /// </summary>
        [HttpGet("Contact")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }

        /// <summary>
        /// Displays the privacy page.
        /// </summary>
        [HttpGet("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page with error details.
        /// </summary>
        [HttpGet("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
