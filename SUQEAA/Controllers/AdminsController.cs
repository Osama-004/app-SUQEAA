using Microsoft.AspNetCore.Mvc;
using WebApplication3.Data; 
using WebApplication3.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore; 
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; 

namespace SUQEAA.Controllers 
{
	[Authorize(Roles = "Admin")] 
	public class AdminsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public AdminsController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Dashboard()
		{
			var viewModel = new DashboardViewModel
			{
				TotalOrders = await _context.Request.CountAsync(), 
				CompletedOrders = await _context.Request.CountAsync(r => r.Status == RequestStatus.Delivered), 
				PendingOrders = await _context.Request.CountAsync(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.InDelivery), 
				TotalAvailableTankCapacity = await _context.Tank.SumAsync(t => t.CurrentCapacity), 

				MostRequestedAreaName = (await _context.Request
					.Include(r => r.Customer)
					.ThenInclude(c => c.Region)
					.Where(r => r.Customer.Region != null) 
					.GroupBy(r => r.Customer.Region.Name)
					.Select(g => new { AreaName = g.Key, Count = g.Count() })
					.OrderByDescending(x => x.Count)
					.FirstOrDefaultAsync())?.AreaName ?? "غير محدد",
				MostRequestedAreaCount = (await _context.Request
					.Include(r => r.Customer)
					.ThenInclude(c => c.Region)
					.Where(r => r.Customer.Region != null) 
					.GroupBy(r => r.Customer.Region.Name)
					.Select(g => new { AreaName = g.Key, Count = g.Count() })
					.OrderByDescending(x => x.Count)
					.FirstOrDefaultAsync())?.Count ?? 0
			};

			return View(viewModel);
		}
	}
}