using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // لاستخدام SelectList و SelectListItem
using WebApplication3.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // لتأمين المتحكم
using System; // لإضافة Exception

namespace SaqqaProject.Controllers // تأكد من مطابقة مساحة الاسم لمشروعك
{
	[Authorize(Roles = "Customer, Admin, Distributor")] // صلاحية عامة للمتحكم للعرض
	public class RequestsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public RequestsController(ApplicationDbContext context)
		{
			_context = context;
		}

		// لوحة تحكم العميل: عرض طلباتي
		[Authorize(Roles = "Customer")] // فقط الزبائن يمكنهم رؤية طلباتهم
		public async Task<IActionResult> MyRequests()
		{
			var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

			if (customerIdClaim == null)
			{
				return RedirectToAction("Login", "Account");
			}

			int currentCustomerId = int.Parse(customerIdClaim.Value);

			var requests = await _context.Request
				.Where(r => r.CustomerId == currentCustomerId)
				.Include(r => r.Customer)
				.Include(r => r.Driver)
				.Include(r => r.Tank)
				.OrderByDescending(r => r.RequestDate)
				.ToListAsync();

			// توجه إلى Views/Customers/Dashboard.cshtml لعرض طلبات العميل
			return View("~/Views/Customers/Dashboard.cshtml", requests);
		}

		// GET: Requests/Index - عرض جميع الطلبات (للمدير والموزع)
		[Authorize(Roles = "Admin, Distributor")] // فقط للمدير والموزع
		public async Task<IActionResult> Index()
		{
			var requests = await _context.Request
										 .Include(r => r.Customer)
										 .Include(r => r.Tank)
										 .Include(r => r.Driver)
										 .ToListAsync();
			return View(requests);
		}

		// GET: Requests/Details/5 - يعرض تفاصيل طلب
		[Authorize(Roles = "Customer, Admin, Distributor")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();
			var request = await _context.Request
										 .Include(r => r.Customer)
										 .Include(r => r.Tank)
										 .Include(r => r.Driver)
										 .FirstOrDefaultAsync(m => m.Id == id);
			if (request == null)
				return NotFound();
			return View(request);
		}

		// GET: Requests/Create (لإنشاء طلب جديد)
		[Authorize(Roles = "Customer")] // فقط الزبائن يمكنهم إنشاء طلبات
		public async Task<IActionResult> Create()
		{
			var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
			if (customerIdClaim == null)
			{
				return RedirectToAction("Login", "Account");
			}
			int currentCustomerId = int.Parse(customerIdClaim.Value);

			var customer = await _context.Customer
										 .Include(c => c.Region)
										 .Include(c => c.District)
										 .FirstOrDefaultAsync(c => c.Id == currentCustomerId);

			if (customer == null)
			{
				return NotFound("بيانات العميل غير متوفرة. يرجى التأكد من التسجيل الصحيح ووجود بيانات المنطقة.");
			}

			var availableTanks = await _context.Tank
											.Where(t => t.DriverId != 0 &&
														(t.RegionId == customer.RegionId ||
														(t.DistrictId.HasValue && t.DistrictId == customer.DistrictId)))
											.OrderBy(t => t.PricePerLiter)
											.ToListAsync();

			var tankOptions = availableTanks.Select(t => new SelectListItem
			{
				Value = t.Id.ToString(),
				Text = $"{t.Name} (السعة: {t.Capacity} لتر، السعر/لتر: {t.PricePerLiter:N2} ل.ت.)"
			}).ToList();
			ViewBag.Tanks = new SelectList(tankOptions, "Value", "Text");

			ViewBag.CustomerId = currentCustomerId;
			return View();
		}


		// POST: Requests/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Customer")]
		public async Task<IActionResult> Create([Bind("Quantity,TankId")] Request request)
		{
			var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
			if (customerIdClaim == null)
			{
				return RedirectToAction("Login", "Account");
			}
			int currentCustomerId = int.Parse(customerIdClaim.Value);

			request.CustomerId = currentCustomerId;
			request.RequestDate = DateTime.Now;

			var selectedTank = await _context.Tank.FindAsync(request.TankId);
			if (selectedTank == null || selectedTank.DriverId == 0)
			{
				ModelState.AddModelError("", "الخزان المختار غير موجود أو لا يملك سائقًا مخصصًا.");
				var customerError = await _context.Customer.Include(c => c.Region).Include(c => c.District).FirstOrDefaultAsync(c => c.Id == currentCustomerId);
				var availableTanksError = await _context.Tank
												.Where(t => t.DriverId != 0 && (t.RegionId == customerError.RegionId || (t.DistrictId.HasValue && t.DistrictId == customerError.DistrictId)))
												.OrderBy(t => t.PricePerLiter)
												.ToListAsync();
				var tankOptionsError = availableTanksError.Select(t => new SelectListItem
				{
					Value = t.Id.ToString(),
					Text = $"{t.Name} (السعة: {t.Capacity} لتر، السعر/لتر: {t.PricePerLiter:N2} ل.ت.)"
				}).ToList();
				ViewBag.Tanks = new SelectList(tankOptionsError, "Value", "Text", request.TankId);
				ViewBag.CustomerId = currentCustomerId;
				return View(request);
			}
			request.DriverId = selectedTank.DriverId;

			if (selectedTank.CurrentCapacity < request.Quantity)
			{
				ModelState.AddModelError("", "الكمية المطلوبة تتجاوز السعة المتاحة في الخزان.");
				var customerErrorCapacity = await _context.Customer.Include(c => c.Region).Include(c => c.District).FirstOrDefaultAsync(c => c.Id == currentCustomerId);
				var availableTanksErrorCapacity = await _context.Tank
												.Where(t => t.DriverId != 0 && (t.RegionId == customerErrorCapacity.RegionId || (t.DistrictId.HasValue && t.DistrictId == customerErrorCapacity.DistrictId)))
												.OrderBy(t => t.PricePerLiter)
												.ToListAsync();
				var tankOptionsErrorCapacity = availableTanksErrorCapacity.Select(t => new SelectListItem
				{
					Value = t.Id.ToString(),
					Text = $"{t.Name} (السعة: {t.Capacity} لتر، السعر/لتر: {t.PricePerLiter:N2} ل.ت.)"
				}).ToList();
				ViewBag.Tanks = new SelectList(tankOptionsErrorCapacity, "Value", "Text", request.TankId);
				ViewBag.CustomerId = currentCustomerId;
				return View(request);
			}

			request.Status = RequestStatus.InDelivery;
			selectedTank.CurrentCapacity -= request.Quantity;

			_context.Add(request);
			_context.Update(selectedTank);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم تقديم طلبك وقبوله تلقائياً! الطلب قيد التوصيل.";
			return RedirectToAction(nameof(MyRequests));

			var customerForView = await _context.Customer.Include(c => c.Region).Include(c => c.District).FirstOrDefaultAsync(c => c.Id == currentCustomerId);
			var tanksForView = await _context.Tank
											.Where(t => t.DriverId != 0 && (t.RegionId == customerForView.RegionId || (t.DistrictId.HasValue && t.DistrictId == customerForView.DistrictId)))
											.OrderBy(t => t.PricePerLiter)
											.ToListAsync();
			var tanksForViewOptions = tanksForView.Select(t => new SelectListItem
			{
				Value = t.Id.ToString(),
				Text = $"{t.Name} (السعة: {t.Capacity} لتر، السعر/لتر: {t.PricePerLiter:N2} ل.ت.)"
			}).ToList();
			ViewBag.Tanks = new SelectList(tanksForViewOptions, "Value", "Text", request.TankId);
			ViewBag.CustomerId = currentCustomerId;

			return View(request);
		}

		// GET: Requests/Edit/5 - يعرض نموذج تعديل طلب
		[Authorize(Roles = "Admin, Distributor")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return NotFound();
			var request = await _context.Request
										.Include(r => r.Customer)
										.Include(r => r.Tank)
										.Include(r => r.Driver)
										.FirstOrDefaultAsync(m => m.Id == id);
			if (request == null)
				return NotFound();
			// تمرير القوائم المنسدلة اللازمة
			ViewBag.CustomerId = new SelectList(await _context.Customer.ToListAsync(), "Id", "Name", request.CustomerId);
			ViewBag.TankId = new SelectList(await _context.Tank.ToListAsync(), "Id", "Name", request.TankId);
			ViewBag.DriverId = new SelectList(await _context.Driver.ToListAsync(), "Id", "Name", request.DriverId);
			return View(request); // هذا سيبحث عن Views/Requests/Edit.cshtml
		}

		// POST: Requests/Edit/5 - يعالج بيانات نموذج تعديل طلب
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin, Distributor")]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Quantity,RequestDate,DeliveryDate,Status,CustomerId,TankId,DriverId")] Request request)
		{
			if (id != request.Id)
				return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(request);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم تعديل الطلب بنجاح.";
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!RequestExists(request.Id))
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
				return RedirectToAction(nameof(Index)); // يعود إلى صفحة جميع الطلبات
			}
			// إعادة تعبئة القوائم المنسدلة في حالة وجود أخطاء
			ViewBag.CustomerId = new SelectList(await _context.Customer.ToListAsync(), "Id", "Name", request.CustomerId);
			ViewBag.TankId = new SelectList(await _context.Tank.ToListAsync(), "Id", "Name", request.TankId);
			ViewBag.DriverId = new SelectList(await _context.Driver.ToListAsync(), "Id", "Name", request.DriverId);
			return View(request);
		}

		// GET: Requests/Delete/5 - يعرض صفحة تأكيد حذف طلب
		[Authorize(Roles = "Admin")] // المدير فقط يمكنه الحذف عادةً
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();
			var request = await _context.Request
										 .Include(r => r.Customer)
										 .Include(r => r.Tank)
										 .Include(r => r.Driver)
										 .FirstOrDefaultAsync(m => m.Id == id);
			if (request == null)
				return NotFound();
			return View(request); // هذا سيبحث عن Views/Requests/Delete.cshtml
		}

		// POST: Requests/Delete/5 - يعالج عملية حذف طلب بعد التأكيد
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")] // المدير فقط
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var request = await _context.Request.FindAsync(id);
			if (request != null)
			{
				// قد تحتاج إلى إعادة الكمية إلى الخزان إذا تم إلغاء الطلب وهو قيد التوصيل
				// if (request.Status == RequestStatus.InDelivery || request.Status == RequestStatus.Pending)
				// {
				//     var tank = await _context.Tank.FindAsync(request.TankId);
				//     if (tank != null)
				//     {
				//         tank.CurrentCapacity += request.Quantity;
				//         _context.Update(tank);
				//     }
				// }
				_context.Request.Remove(request);
			}
			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "تم حذف الطلب بنجاح.";
			return RedirectToAction(nameof(Index));
		}

		// دالة لإلغاء الطلب بواسطة العميل - تم تعديلها للسماح بالإلغاء من حالة "قيد التوصيل"
		[Authorize(Roles = "Customer")]
		public async Task<IActionResult> Cancel(int id)
		{
			var request = await _context.Request.FindAsync(id);
			var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

			if (request == null || customerIdClaim == null || request.CustomerId != int.Parse(customerIdClaim.Value))
			{
				return NotFound();
			}

			// السماح بالإلغاء إذا كان قيد الانتظار أو قيد التوصيل
			if (request.Status == RequestStatus.Pending || request.Status == RequestStatus.InDelivery)
			{
				request.Status = RequestStatus.Cancelled;

				// إذا كان الطلب قيد التوصيل، يجب إعادة الكمية إلى الخزان
				if (request.Status == RequestStatus.InDelivery) // قبل تغيير الحالة إلى Cancelled
				{
					var tank = await _context.Tank.FindAsync(request.TankId);
					if (tank != null)
					{
						tank.CurrentCapacity += request.Quantity; // إعادة الكمية إلى الخزان
						_context.Update(tank);
					}
				}

				_context.Update(request);
				await _context.SaveChangesAsync();
				TempData["SuccessMessage"] = "تم إلغاء طلبك بنجاح.";
			}
			else
			{
				TempData["ErrorMessage"] = "لا يمكن إلغاء الطلب في حالته الحالية.";
			}
			return RedirectToAction(nameof(MyRequests));
		}

		// دالة لقبول الطلب بواسطة الموزع/المدير
		// هذه الدالة لن تكون ضرورية للطلبات الجديدة، لكن قد تُستخدم للطلبات التي تم رفضها أو تعديلها يدوياً
		[Authorize(Roles = "Admin, Distributor")]
		public async Task<IActionResult> Accept(int id, int driverId)
		{
			var request = await _context.Request.FindAsync(id);
			if (request == null || request.Status != RequestStatus.Pending)
			{
				return NotFound();
			}

			var driver = await _context.Driver.FindAsync(driverId);
			if (driver == null)
			{
				TempData["ErrorMessage"] = "السائق المحدد غير موجود.";
				return RedirectToAction(nameof(Index));
			}

			var tank = await _context.Tank.FindAsync(request.TankId);
			if (tank == null || tank.CurrentCapacity < request.Quantity)
			{
				TempData["ErrorMessage"] = "الكمية المطلوبة تتجاوز السعة المتاحة في الخزان.";
				request.Status = RequestStatus.Cancelled;
				_context.Update(request);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			request.Status = RequestStatus.InDelivery;
			request.DriverId = driverId;

			tank.CurrentCapacity -= request.Quantity;
			_context.Update(request);
			_context.Update(tank);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم قبول الطلب وتعيين السائق بنجاح.";
			return RedirectToAction(nameof(Index));
		}

		// دالة لإكمال الطلب بواسطة السائق/الموزع/المدير
		[Authorize(Roles = "Admin, Distributor")]
		public async Task<IActionResult> Complete(int id)
		{
			var request = await _context.Request.FindAsync(id);
			if (request == null || request.Status != RequestStatus.InDelivery)
			{
				return NotFound();
			}

			request.Status = RequestStatus.Delivered;
			request.DeliveryDate = DateTime.Now;
			_context.Update(request);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم إكمال الطلب بنجاح.";
			return RedirectToAction(nameof(Index));
		}

		private bool RequestExists(int id)
		{
			return _context.Request.Any(e => e.Id == id);
		}
	}
}