using Endpoint.Site.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Endpoint.Site.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            var token = Request.Cookies["AccessToken"];

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Authentication");
            }

            ViewBag.AccessToken = token;
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
