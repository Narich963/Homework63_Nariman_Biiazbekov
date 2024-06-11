using Microsoft.AspNetCore.Mvc;
using MyChat.Models;
using System.Diagnostics;

namespace MyChat.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}
		[ResponseCache(CacheProfileName = "Caching")]
		public IActionResult Index()
		{
			return View();
		}
        [ResponseCache(CacheProfileName = "Caching")]
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
