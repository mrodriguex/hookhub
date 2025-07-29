using HookHub.Core.Hooks;
using HookHub.Core.Hubs;
using HookHub.Hub.Models;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HookHub.Hub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HubController : Controller
    {
        private CoreHookInfoModel _netClientInfo;
        private CoreHook _netClient;

        public CoreHook CoreHook
        {
            get
            {
                _netClient ??= new CoreHook();
                return (_netClient);
            }
            set { _netClient = value; }
        }

        public CoreHub CoreHub { get; private set; }

        public CoreHookInfoModel CoreHookInfo
        {
            get
            {
                _netClientInfo ??= new CoreHookInfoModel(CoreHook);
                return (_netClientInfo);
            }
            set { _netClientInfo = value; }
        }

        public HubController(CoreHub coreHub, CoreHook netClient)
        {
            CoreHook = netClient;
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

        [HttpGet("{Action}/{connectionID}")]
        public IActionResult PurgeDisconnection(string connectionID)
        {
            return Json(CoreHub.PurgeDisconnection(connectionID: connectionID));
        }
    }
}
