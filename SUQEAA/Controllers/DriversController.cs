// في DriversController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models; // تأكد من مساحة الاسم الصحيحة لنماذجك
using WebApplication3.Data;   // تأكد من مساحة الاسم الصحيحة لسياق البيانات
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // لتأمين المتحكم
using System; // لإضافة Exception

namespace SaqqaProject.Controllers // تأكد من مساحة الاسم الصحيحة لمتحكماتك
{
	[Authorize(Roles = "Admin")] // المدير فقط هو من يدير السائقين
	public class DriversController : Controller
	{
		private readonly ApplicationDbContext _context;

		public DriversController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Drivers
		// يعرض قائمة بجميع السائقين
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Driver
											   .Include(d => d.Tank); // تضمين الخزان المرتبط للعرض
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: Drivers/Details/5
		// يعرض تفاصيل سائق معين
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var driver = await _context.Driver
									   .Include(d => d.Tank) // تضمين الخزان المرتبط
									   .Include(d => d.Requests) // تضمين الطلبات المرتبطة
									   .FirstOrDefaultAsync(m => m.Id == id);
			if (driver == null)
			{
				return NotFound();
			}
			return View(driver);
		}

		// GET: Drivers/Create
		// يعرض نموذج إنشاء سائق جديد
		public IActionResult Create()
		{
			return View();
		}

		// POST: Drivers/Create
		// يعالج بيانات نموذج إنشاء سائق جديد
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,PhoneNumber,LicenceNumber")] Driver driver)
		{
			if (ModelState.IsValid)
			{
				try
				{
					_context.Add(driver);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم إضافة السائق بنجاح.";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateException ex) // لالتقاط الأخطاء مثل انتهاك القيد الفريد
				{
					ModelState.AddModelError("Name", "اسم السائق موجود بالفعل.");
					// يمكنك تسجيل الخطأ: Console.WriteLine(ex.InnerException?.Message);
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
				}
			}
			return View(driver);
		}

		// GET: Drivers/Edit/5
		// يعرض نموذج تعديل سائق موجود
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var driver = await _context.Driver.FindAsync(id);
			if (driver == null)
			{
				return NotFound();
			}
			return View(driver);
		}

		// POST: Drivers/Edit/5
		// يعالج بيانات نموذج تعديل سائق
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PhoneNumber,LicenceNumber")] Driver driver)
		{
			if (id != driver.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(driver);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم تعديل بيانات السائق بنجاح.";
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DriverExists(driver.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				catch (DbUpdateException ex)
				{
					ModelState.AddModelError("Name", "اسم السائق موجود بالفعل.");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
				}
				return RedirectToAction(nameof(Index));
			}
			return View(driver);
		}

		// GET: Drivers/Delete/5
		// يعرض صفحة تأكيد حذف سائق
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var driver = await _context.Driver
									   .Include(d => d.Tank) // تضمين الخزان المرتبط
									   .Include(d => d.Requests) // تضمين الطلبات المرتبطة
									   .FirstOrDefaultAsync(m => m.Id == id);
			if (driver == null)
			{
				return NotFound();
			}
			return View(driver);
		}

		// POST: Drivers/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var driver = await _context.Driver.FindAsync(id);
			if (driver != null)
			{
				// يجب الانتباه هنا: إذا كان السائق مرتبطًا بخزان (علاقة One-to-One) أو بطلبات
				// فقد يؤدي حذفها إلى خطأ في المفتاح الخارجي. قد تحتاج إلى إزالة التبعيات أولاً.
				var hasRelatedTank = await _context.Tank.AnyAsync(t => t.DriverId == id);
				var hasRelatedRequests = await _context.Request.AnyAsync(r => r.DriverId == id);

				if (hasRelatedTank || hasRelatedRequests)
				{
					TempData["ErrorMessage"] = "لا يمكن حذف السائق لوجود خزان أو طلبات مرتبطة به.";
				}
				else
				{
					_context.Driver.Remove(driver);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم حذف السائق بنجاح.";
				}
			}
			else
			{
				TempData["ErrorMessage"] = "السائق غير موجود.";
			}
			return RedirectToAction(nameof(Index));
		}

		// دالة مساعدة للتحقق مما إذا كان السائق موجودًا
		private bool DriverExists(int id)
		{
			return _context.Driver.Any(e => e.Id == id);
		}
	}
}