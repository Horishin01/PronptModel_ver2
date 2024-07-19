using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PronptModel_ver2.Models;
using System.Diagnostics;

namespace PronptModel_ver2.Controllers
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

        // AccessErrorƒAƒNƒVƒ‡ƒ“(GET‚Ì‚Ý)
        public IActionResult AccessError(int? id)
        {
            if (id == null)
                return NotFound();

            int statusCode = id.Value;

            ViewBag.StatusCode = statusCode;
            ViewBag.ReasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);

            return View();
        }
    }
}
