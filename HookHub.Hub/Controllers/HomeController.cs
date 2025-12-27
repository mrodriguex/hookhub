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
        /// <summary>
        /// Displays the home page.
        /// </summary>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
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
