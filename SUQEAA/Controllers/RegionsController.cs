using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models; // تأكد من مساحة الاسم الصحيحة لنماذجك
using WebApplication3.Data;   // تأكد من مساحة الاسم الصحيحة لسياق البيانات
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // لتأمين المتحكم
using System; // لإضافة Exception للتعامل مع الأخطاء العامة

namespace SaqqaProject.Controllers // تأكد من مساحة الاسم الصحيحة لمتحكماتك
{
	// المتحكم الخاص بإدارة المناطق
	[Authorize(Roles = "Admin")] // المدير فقط هو من يدير المناطق
	public class RegionsController : Controller
	{
		// حقل خاص للقراءة فقط لـ ApplicationDbContext
		private readonly ApplicationDbContext _context;

		// المُنشئ الذي يقوم بحقن ApplicationDbContext
		public RegionsController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Regions
		// يعرض قائمة بجميع المناطق
		public async Task<IActionResult> Index()
		{
			// يجلب جميع المناطق من قاعدة البيانات ويعرضها في الواجهة
			return View(await _context.Region.ToListAsync());
		}

		// GET: Regions/Details/5 - تم التعديل هنا لتضمين المناطق الفرعية
		// يعرض تفاصيل منطقة معينة بناءً على معرفها (ID)
		public async Task<IActionResult> Details(int? id)
		{
			// التحقق مما إذا كان المعرف فارغاً
			if (id == null)
			{
				return NotFound(); // يعيد خطأ 404 إذا لم يتم توفير معرف
			}

			// البحث عن المنطقة في قاعدة البيانات بناءً على المعرف وتضمين المناطق الفرعية المرتبطة
			var region = await _context.Region
				.Include(r => r.Districts) // <<<<< تم إضافة هذا السطر لتضمين المناطق الفرعية
										   // .Include(r => r.Customers) // يمكن إضافة هذا السطر لتضمين الزبائن المرتبطين إذا لزم الأمر
				.FirstOrDefaultAsync(m => m.Id == id);
			// التحقق مما إذا تم العثور على المنطقة
			if (region == null)
			{
				return NotFound(); // يعيد خطأ 404 إذا لم يتم العثور على المنطقة
			}

			return View(region); // يعرض تفاصيل المنطقة في الواجهة
		}

		// GET: Regions/Create
		// يعرض نموذج إنشاء منطقة جديدة
		public IActionResult Create()
		{
			return View(); // يعرض واجهة "Create" الفارغة للمستخدم
		}

		// POST: Regions/Create
		// يعالج بيانات نموذج إنشاء منطقة جديدة بعد إرسالها
		[HttpPost] // يشير إلى أن هذه الدالة تستجيب لطلبات POST
		[ValidateAntiForgeryToken] // حماية ضد هجمات تزوير الطلبات عبر المواقع (CSRF)
		public async Task<IActionResult> Create([Bind("Id,Name")] Region region)
		{
			// التحقق من صحة بيانات النموذج (بناءً على DataAnnotations في نموذج Region)
			if (ModelState.IsValid)
			{
				try
				{
					_context.Region.Add(region); // يضيف الكائن Region إلى DbSet الخاص بالمناطق
					await _context.SaveChangesAsync(); // يحفظ التغييرات في قاعدة البيانات
					TempData["SuccessMessage"] = "تم إضافة المنطقة بنجاح."; // رسالة نجاح
					return RedirectToAction(nameof(Index)); // يعيد توجيه المستخدم إلى صفحة قائمة المناطق
				}
				catch (DbUpdateException ex) // التقاط أخطاء قاعدة البيانات المحددة (مثل انتهاك قيود UNIQUE)
				{
					// هذا يمكن أن يحدث إذا كان هناك قيد فريد على الاسم وحاولت إدخال اسم موجود بالفعل
					// يمكنك فحص InnerException لرسالة أكثر تفصيلاً
					ModelState.AddModelError("Name", "اسم المنطقة موجود بالفعل.");
					// يمكنك تسجيل الخطأ: Console.WriteLine(ex.InnerException?.Message);
				}
				catch (Exception ex) // التقاط الأخطاء العامة الأخرى
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
					// يمكنك تسجيل الخطأ هنا
				}
			}
			return View(region); // إذا كان النموذج غير صالح، يعيد عرض نفس الواجهة مع أخطاء التحقق
		}

		// GET: Regions/Edit/5
		// يعرض نموذج تعديل منطقة موجودة بناءً على معرفها
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			// البحث عن المنطقة في قاعدة البيانات
			var region = await _context.Region.FindAsync(id);
			if (region == null)
			{
				return NotFound();
			}
			return View(region); // يعرض واجهة "Edit" مع بيانات المنطقة الحالية
		}

		// POST: Regions/Edit/5
		// يعالج بيانات نموذج تعديل منطقة بعد إرسالها
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Region region)
		{
			// التحقق من تطابق المعرف في الرابط مع المعرف في النموذج
			if (id != region.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(region); // يقوم بتحديث الكائن Region في DbSet
					await _context.SaveChangesAsync(); // يحفظ التغييرات في قاعدة البيانات
					TempData["SuccessMessage"] = "تم تعديل المنطقة بنجاح."; // رسالة نجاح
				}
				catch (DbUpdateConcurrencyException)
				{
					// يتعامل مع مشاكل التزامن إذا تم تعديل السجل من قبل مستخدم آخر في نفس الوقت
					if (!RegionExists(region.Id))
					{
						return NotFound();
					}
					else
					{
						throw; // يعيد رمي الاستثناء إذا كانت مشكلة أخرى
					}
				}
				catch (DbUpdateException ex) // التقاط أخطاء قاعدة البيانات المحددة (مثل انتهاك قيود UNIQUE)
				{
					// هذا يمكن أن يحدث إذا كان هناك قيد فريد على الاسم وحاولت تغيير الاسم إلى اسم موجود بالفعل
					ModelState.AddModelError("Name", "اسم المنطقة موجود بالفعل.");
					// يمكنك تسجيل الخطأ: Console.WriteLine(ex.InnerException?.Message);
				}
				catch (Exception ex) // التقاط الأخطاء العامة
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
					// يمكنك تسجيل الخطأ هنا
				}
				return RedirectToAction(nameof(Index)); // يعيد توجيه المستخدم إلى صفحة قائمة المناطق
			}
			// إعادة عرض النموذج مع أخطاء التحقق إذا لم يكن ModelState صالحاً
			return View(region);
		}

		// GET: Regions/Delete/5
		// يعرض صفحة تأكيد حذف منطقة
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var region = await _context.Region
				.FirstOrDefaultAsync(m => m.Id == id);
			if (region == null)
			{
				return NotFound();
			}

			return View(region); // يعرض واجهة "Delete" لتأكيد الحذف
		}

		// POST: Regions/Delete/5
		// يعالج عملية حذف منطقة بعد تأكيدها
		[HttpPost, ActionName("Delete")] // يشير إلى أن هذه الدالة تستجيب لطلبات POST وتسمى "Delete"
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			// البحث عن المنطقة المراد حذفها
			var region = await _context.Region.FindAsync(id);
			if (region != null)
			{
				// يجب الانتباه هنا: إذا كانت هناك مناطق فرعية أو زبائن مرتبطين بهذه المنطقة،
				// فقد يؤدي حذفها إلى خطأ في المفتاح الخارجي. قد تحتاج إلى حذف التبعيات أولاً.
				// يمكنك التحقق من وجود تبعيات أولاً:
				var hasRelatedDistricts = await _context.District.AnyAsync(d => d.RegionId == id);
				var hasRelatedCustomers = await _context.Customer.AnyAsync(c => c.RegionId == id);

				if (hasRelatedDistricts || hasRelatedCustomers)
				{
					TempData["ErrorMessage"] = "لا يمكن حذف المنطقة لوجود مناطق فرعية أو زبائن مرتبطين بها.";
				}
				else
				{
					_context.Region.Remove(region); // يزيل الكائن Region من DbSet
					await _context.SaveChangesAsync(); // يحفظ التغييرات في قاعدة البيانات (حذف السجل)
					TempData["SuccessMessage"] = "تم حذف المنطقة بنجاح."; // رسالة نجاح
				}
			}
			else
			{
				TempData["ErrorMessage"] = "المنطقة غير موجودة."; // رسالة خطأ إذا لم يتم العثور على المنطقة
			}
			return RedirectToAction(nameof(Index)); // يعيد توجيه المستخدم إلى صفحة قائمة المناطق
		}

		// دالة مساعدة للتحقق مما إذا كانت المنطقة موجودة
		private bool RegionExists(int id)
		{
			return _context.Region.Any(e => e.Id == id);
		}
	}
}