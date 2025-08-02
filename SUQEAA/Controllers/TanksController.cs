using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models; // تأكد من مساحة الاسم الصحيحة لنماذجك
using WebApplication3.Data;   // تأكد من مساحة الاسم الصحيحة لسياق البيانات
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // لتأمين المتحكم
using System; // لإضافة Exception
using Microsoft.AspNetCore.Mvc.Rendering; // لإضافة SelectList

namespace SaqqaProject.Controllers // تأكد من مساحة الاسم الصحيحة لمتحكماتك
{
	[Authorize(Roles = "Admin, Distributor")] // المدير والموزع يمكنهم إدارة الخزانات
	public class TanksController : Controller // <<<<<<< تم التأكد من أن اسم الكلاس هو TanksController
	{
		private readonly ApplicationDbContext _context;

		public TanksController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Tanks
		// يعرض قائمة بجميع الخزانات
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Tank
											   .Include(t => t.Region)
											   .Include(t => t.District)
											   .Include(t => t.Driver); // تضمين السائق
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: Tanks/Details/5 - يعرض تفاصيل خزان معين
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var tank = await _context.Tank
									 .Include(t => t.Region)
									 .Include(t => t.District)
									 .Include(t => t.Driver)
									 .FirstOrDefaultAsync(m => m.Id == id);
			if (tank == null)
			{
				return NotFound();
			}
			return View(tank);
		}

		// GET: Tanks/Create - يعرض نموذج إنشاء خزان جديد
		public async Task<IActionResult> Create()
		{
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name");
			ViewBag.DistrictId = new SelectList(await _context.District.ToListAsync(), "Id", "Name"); // يمكن فلترتها بـ JavaScript
																									  // جلب السائقين الذين لم يُربطوا بخزان بعد (بسبب العلاقة One-to-One)
			ViewBag.DriverId = new SelectList(await _context.Driver
															.Where(d => !_context.Tank.Any(t => t.DriverId == d.Id))
															.ToListAsync(), "Id", "Name");
			return View();
		}

		// POST: Tanks/Create - يعالج بيانات نموذج إنشاء خزان جديد
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,Capacity,CurrentCapacity,Location,PricePerLiter,RegionId,DistrictId,DriverId")] Tank tank)
		{
			if (ModelState.IsValid)
			{
				try
				{
					_context.Add(tank);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم إضافة الخزان بنجاح.";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateException ex) // لالتقاط أخطاء قاعدة البيانات المحددة (مثل انتهاك القيد الفريد)
				{
					ModelState.AddModelError("", $"حدث خطأ في قاعدة البيانات: {ex.InnerException?.Message}");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
				}
			}
			// إعادة تعبئة ViewBags في حالة وجود أخطاء
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", tank.RegionId);
			ViewBag.DistrictId = new SelectList(await _context.District.ToListAsync(), "Id", "Name", tank.DistrictId);
			ViewBag.DriverId = new SelectList(await _context.Driver
															.Where(d => !_context.Tank.Any(tItem => tItem.DriverId == d.Id && tItem.Id != tank.Id)) // تم تصحيح هذا السطر أيضاً
															.ToListAsync(), "Id", "Name", tank.DriverId);
			return View(tank);
		}

		// GET: Tanks/Edit/5 - يعرض نموذج تعديل خزان
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var tank = await _context.Tank.FindAsync(id);
			if (tank == null)
			{
				return NotFound();
			}
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", tank.RegionId);
			ViewBag.DistrictId = new SelectList(await _context.District.ToListAsync(), "Id", "Name", tank.DistrictId);
			// جلب السائقين المتاحين (الذين ليسوا مرتبطين بخزان آخر) + السائق الحالي لهذا الخزان (باستخدام tank.DriverId)
			ViewBag.DriverId = new SelectList(await _context.Driver
															.Where(d => !_context.Tank.Any(tItem => tItem.DriverId == d.Id && tItem.Id != tank.Id) || d.Id == tank.DriverId) // <<<<< تم تصحيح هذا السطر
															.ToListAsync(), "Id", "Name", tank.DriverId);
			return View(tank);
		}

		// POST: Tanks/Edit/5 - يعالج بيانات نموذج تعديل خزان
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Capacity,CurrentCapacity,Location,PricePerLiter,RegionId,DistrictId,DriverId")] Tank tank)
		{
			if (id != tank.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					// للتأكد من تحديث الكيان:
					_context.Entry(tank).State = EntityState.Modified; // تعيين حالة الكيان صراحة إلى Modified

					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم تعديل الخزان بنجاح.";
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!TankExists(tank.Id))
						return NotFound();
					else
						throw;
				}
				catch (DbUpdateException ex)
				{
					ModelState.AddModelError("", $"حدث خطأ في قاعدة البيانات: {ex.InnerException?.Message}");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
				}
				return RedirectToAction(nameof(Index));
			}
			// إعادة تعبئة ViewBags في حالة وجود أخطاء
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", tank.RegionId);
			ViewBag.DistrictId = new SelectList(await _context.District.ToListAsync(), "Id", "Name", tank.DistrictId);
			ViewBag.DriverId = new SelectList(await _context.Driver
															.Where(d => !_context.Tank.Any(tItem => tItem.DriverId == d.Id && tItem.Id != tank.Id) || d.Id == tank.DriverId)
															.ToListAsync(), "Id", "Name", tank.DriverId);
			return View(tank);
		}

		// GET: Tanks/Delete/5 - يعرض صفحة تأكيد حذف خزان
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var tank = await _context.Tank
									 .Include(t => t.Region)
									 .Include(t => t.District)
									 .Include(t => t.Driver)
									 .FirstOrDefaultAsync(m => m.Id == id);
			if (tank == null)
			{
				return NotFound();
			}
			return View(tank);
		}

		// POST: Tanks/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var tank = await _context.Tank.FindAsync(id);
			if (tank != null)
			{
				// تحقق من عدم وجود طلبات مرتبطة بهذا الخزان قبل الحذف
				var hasRelatedRequests = await _context.Request.AnyAsync(r => r.TankId == id);
				if (hasRelatedRequests)
				{
					TempData["ErrorMessage"] = "لا يمكن حذف الخزان لوجود طلبات مرتبطة به.";
				}
				else
				{
					_context.Tank.Remove(tank);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم حذف الخزان بنجاح.";
				}
			}
			else
			{
				TempData["ErrorMessage"] = "الخزان غير موجود.";
			}
			return RedirectToAction(nameof(Index));
		}

		// دالة مساعدة للتحقق مما إذا كان الخزان موجوداً
		private bool TankExists(int id)
		{
			return _context.Tank.Any(e => e.Id == id);
		}
	}
}