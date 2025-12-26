using HookHub.Core.Hooks;
using HookHub.Core.Hubs;
using HookHub.Core.Workers;
using HookHub.Hub.Models;

using Microsoft.AspNetCore.Mvc;

namespace HookHub.Hub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HubController : Controller
    {
        private CoreHookInfoModel _netClientInfo;

        public Worker Worker { get; set; }

        public CoreHub CoreHub { get; private set; }

        public CoreHookInfoModel CoreHookInfo
        {
            get
            {
                _netClientInfo ??= new CoreHookInfoModel(Worker.Hook);
                return (_netClientInfo);
            }
            set { _netClientInfo = value; }
        }

        public HubController(CoreHub coreHub, Worker worker)
        {
            Worker = worker;
            CoreHub = coreHub;
        }

        [HttpGet("{Action}")]
        public IActionResult Index()
        {
            return View(CoreHookInfo);
        }


        [HttpGet("{Action}")]
        public IActionResult GetAllHookConnections()
        {
            return Json(CoreHub.GetAllHookConnections());
        }


        [HttpGet("{Action}")]
        public IActionResult PurgeDisconnections()
        {
            return Json(CoreHub.PurgeDisconnections());
        }

        [HttpGet("{Action}/{connectionId}")]
        public IActionResult PurgeDisconnection(string connectionId)
        {
            return Json(CoreHub.PurgeDisconnection(connectionId: connectionId));
        }
    }
}
