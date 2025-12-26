using HookHub.Core.Workers;
using Microsoft.AspNetCore.Mvc;

namespace HookHub.Hook.Controllers
{
    public class HomeController : Controller
    {
        private readonly Worker _worker;

        public HomeController(Worker worker)
        {
            _worker = worker;
        }

        public IActionResult Index()
        {
            return View(_worker);
        }
    }
}
