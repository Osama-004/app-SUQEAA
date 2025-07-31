using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

namespace SaqqaProject.Controllers
{
	[Authorize(Roles = "Distributor")] 
	public class DistributorsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public DistributorsController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Dashboard()
		{
			var driverIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
			if (driverIdClaim == null)
			{
				return RedirectToAction("Login", "Account");
			}
			int currentDriverId = int.Parse(driverIdClaim.Value);

			var driverRequests = await _context.Request
											   .Where(r => r.DriverId == currentDriverId)
											   .Include(r => r.Customer)
												   .ThenInclude(c => c.Region)
											   .Include(r => r.Tank)
											   .Include(r => r.Driver)
											   .OrderByDescending(r => r.RequestDate)
											   .ToListAsync();

			var viewModel = new DistributorDashboardViewModel
			{
				PendingRequests = driverRequests.Where(r => r.Status == RequestStatus.InDelivery).ToList(),
				CompletedRequests = driverRequests.Where(r => r.Status == RequestStatus.Delivered).ToList(),
				TotalRequestsCount = driverRequests.Count,
				PendingRequestsCount = driverRequests.Count(r => r.Status == RequestStatus.InDelivery)
			};

			return View(viewModel);
		}

		
	}
}