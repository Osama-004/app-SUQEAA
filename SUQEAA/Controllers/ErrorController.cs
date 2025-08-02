using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
	public class ErrorController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult NotFound()
		{
			return View();
		}
	}
}