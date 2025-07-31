using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models; 
using WebApplication3.Data;  
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace SaqqaProject.Controllers 
{
	[Authorize(Roles = "Admin, Distributor")] 
	public class DistrictsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public DistrictsController(ApplicationDbContext context)
		{
			_context = context;
		}

		
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.District.Include(d => d.Region);
			return View(await applicationDbContext.ToListAsync());
		}

		
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var district = await _context.District
										 .Include(d => d.Region) 
										 .Include(d => d.Customers) 
										 .FirstOrDefaultAsync(m => m.Id == id);
			if (district == null)
			{
				return NotFound();
			}
			return View(district);
		}

		
		public async Task<IActionResult> Create()
		{
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name"); 
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,RegionId")] District district)
		{
			if (ModelState.IsValid)
			{
				try
				{
					_context.Add(district);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم إضافة المنطقة الفرعية بنجاح.";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateException ex) 
				{
					ModelState.AddModelError("Name", "اسم المنطقة الفرعية موجود بالفعل في هذه المنطقة.");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
				}
			}
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", district.RegionId); // إعادة تمرير المناطق في حالة الخطأ
			return View(district);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var district = await _context.District.FindAsync(id);
			if (district == null)
			{
				return NotFound();
			}
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", district.RegionId); 
			return View(district);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,RegionId")] District district)
		{
			if (id != district.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(district);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم تعديل المنطقة الفرعية بنجاح.";
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DistrictExists(district.Id))
						return NotFound();
					else
						throw;
				}
				catch (DbUpdateException ex)
				{
					ModelState.AddModelError("Name", "اسم المنطقة الفرعية موجود بالفعل في هذه المنطقة.");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
				}
				return RedirectToAction(nameof(Index));
			}
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", district.RegionId);
			return View(district);
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var district = await _context.District
										 .Include(d => d.Region)
										 .Include(d => d.Customers)
										 .FirstOrDefaultAsync(m => m.Id == id);
			if (district == null)
			{
				return NotFound();
			}
			return View(district);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var district = await _context.District.FindAsync(id);
			if (district != null)
			{
				var hasRelatedCustomers = await _context.Customer.AnyAsync(c => c.DistrictId == id);
				var hasRelatedTanks = await _context.Tank.AnyAsync(t => t.DistrictId == id);

				if (hasRelatedCustomers || hasRelatedTanks)
				{
					TempData["ErrorMessage"] = "لا يمكن حذف المنطقة الفرعية لوجود زبائن أو خزانات مرتبطة بها.";
				}
				else
				{
					_context.District.Remove(district);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم حذف المنطقة الفرعية بنجاح.";
				}
			}
			else
			{
				TempData["ErrorMessage"] = "المنطقة الفرعية غير موجودة.";
			}
			return RedirectToAction(nameof(Index));
		}

		private bool DistrictExists(int id)
		{
			return _context.District.Any(e => e.Id == id);
		}
	}
}