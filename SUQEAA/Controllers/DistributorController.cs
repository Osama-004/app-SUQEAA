using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;

namespace SaqiaProject.Controllers
{
	[Authorize(Roles = "Distributor")]

	public class DistrictController : Controller
	{
		private readonly ApplicationDbContext _context;

		public DistrictController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			return View(await _context.District.ToListAsync());
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			var district = await _context.District
										 .Include(d => d.Region)
										 .Include(d => d.Customers)
										 .FirstOrDefaultAsync(m => m.Id == id);

			if (district == null)
				return NotFound();

			return View(district);
		}

		public IActionResult Create()
		{
			ViewBag.RegionId = new SelectList(_context.Region, "Id", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,RegionId")] District district)
		{
			if (ModelState.IsValid)
			{
				_context.Add(district);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewBag.RegionId = new SelectList(_context.Region, "Id", "Name", district.RegionId);
			return View(district);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return NotFound();

			var district = await _context.District.FindAsync(id);
			if (district == null)
				return NotFound();

			ViewBag.RegionId = new SelectList(_context.Region, "Id", "Name", district.RegionId);
			return View(district);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,RegionId")] District district)
		{
			if (id != district.Id)
				return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(district);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DistrictExists(district.Id))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}
			ViewBag.RegionId = new SelectList(_context.Region, "Id", "Name", district.RegionId);
			return View(district);
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();

			var district = await _context.District
										 .Include(d => d.Region)
										 .Include(d => d.Customers)
										 .FirstOrDefaultAsync(m => m.Id == id);

			if (district == null)
				return NotFound();

			return View(district);
		}
		public IActionResult Dashboard()
		{
			return View(); 
		}
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var district = await _context.District.FindAsync(id);
			if (district != null)
			{
				_context.District.Remove(district);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		private bool DistrictExists(int id)
		{
			return _context.District.Any(e => e.Id == id);
		}
	}
}
