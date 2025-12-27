using HookHub.Core.Hooks;
using HookHub.Core.Hubs;
using HookHub.Core.Workers;
using HookHub.Hub.Models;

using Microsoft.AspNetCore.Mvc;

namespace HookHub.Hub.Controllers
{
    /// <summary>
    /// API controller for managing the hub service and hook connections.
    /// Provides endpoints to view hub status, manage hook connections, and purge disconnections.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HubController : Controller
    {
        /// <summary>
        /// Cached model containing hook information.
        /// </summary>
        private CoreHookInfoModel _netClientInfo;

        /// <summary>
        /// The background worker managing the hub.
        /// </summary>
        public Worker Worker { get; set; }

        /// <summary>
        /// The SignalR hub managing client connections.
        /// </summary>
        public CoreHub CoreHub { get; private set; }

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
        public HubController(CoreHub coreHub, Worker worker)
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
            return View(CoreHookInfo);
        }

        /// <summary>
        /// Gets all current hook connections.
        /// </summary>
        /// <returns>JSON array of all hook connections.</returns>
        [HttpGet("{Action}")]
        public IActionResult GetAllHookConnections()
        {
            return Json(CoreHub.GetAllHookConnections());
        }

        /// <summary>
        /// Purges all disconnected hook connections.
        /// </summary>
        /// <returns>JSON result of the purge operation.</returns>
        [HttpGet("{Action}")]
        public IActionResult PurgeDisconnections()
        {
            return Json(CoreHub.PurgeDisconnections());
        }

        /// <summary>
        /// Purges a specific disconnected hook connection by connection ID.
        /// </summary>
        /// <param name="connectionId">The connection ID to purge.</param>
        /// <returns>JSON result of the purge operation.</returns>
        [HttpGet("{Action}/{connectionId}")]
        public IActionResult PurgeDisconnection(string connectionId)
        {
            return Json(CoreHub.PurgeDisconnection(connectionId: connectionId));
        }
    }
}
