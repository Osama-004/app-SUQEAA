using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using WebApplication3.Data; 
using System.Linq; 
using System.Threading.Tasks;

namespace WebApplication3.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public HomeController(ApplicationDbContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		// ÏÇáÉ áÌáÈ ÇáãäÇØŞ ÇáİÑÚíÉ ÈäÇÁğ Úáì ãÚÑİ ÇáãäØŞÉ
		[HttpGet]
		public async Task<JsonResult> GetDistrictsByRegion(int regionId)
		{
			var districts = await _context.District
										.Where(d => d.RegionId == regionId)
										.Select(d => new { id = d.Id, name = d.Name }) // ÇÑÌÚ ßÇÆäÇÊ ãÌåæáÉ ÊÍÊæí Úáì id æ name
										.ToListAsync();
			return Json(districts);
		}
	}
}