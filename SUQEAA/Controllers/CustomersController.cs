using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication3.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SaqqaProject.Controllers
{
	[Authorize(Roles = "Customer, Admin")]
	public class CustomersController : Controller
	{
		private readonly ApplicationDbContext _context;

		public CustomersController(ApplicationDbContext context)
		{
			_context = context;
		}

		[Authorize(Roles = "Customer")]
		public IActionResult Dashboard()
		{
			return RedirectToAction("MyRequests", "Requests");
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Customer
											   .Include(c => c.Region)
											   .Include(c => c.District)
											   .Include(c => c.SimpleUser);
			return View(await applicationDbContext.ToListAsync());
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var customer = await _context.Customer
										.Include(c => c.Region)
										.Include(c => c.District)
										.Include(c => c.SimpleUser)
										.FirstOrDefaultAsync(m => m.Id == id);
			if (customer == null)
			{
				return NotFound();
			}

			return View(customer);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create()
		{
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name");
			ViewBag.DistrictId = new SelectList(new List<District>(), "Id", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create(string email, string password, int regionId, int districtId)
		{
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", regionId);
			ViewBag.DistrictId = new SelectList(await _context.District.Where(d => d.RegionId == regionId).ToListAsync(), "Id", "Name", districtId);

			var userExists = await _context.SimpleUsers.AnyAsync(s => s.Email == email);
			if (userExists)
			{
				ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم مسبقاً لحساب آخر.");
				return View();
			}

			var newUser = new SimpleUser
			{
				Email = email,
				Password = password,
				Role = "Customer"
			};
			_context.SimpleUsers.Add(newUser);
			await _context.SaveChangesAsync();

			var defaultRegionId = (await _context.Region.FirstOrDefaultAsync())?.Id;
			var defaultDistrictId = (await _context.District.FirstOrDefaultAsync())?.Id;

			if (!defaultRegionId.HasValue || !defaultDistrictId.HasValue)
			{
				ModelState.AddModelError("", "خطأ في الإعداد: لا توجد مناطق أو مناطق فرعية متاحة لتعيينها افتراضياً للزبون. يرجى الاتصال بالدعم.");
				_context.SimpleUsers.Remove(newUser);
				await _context.SaveChangesAsync();
				return View();
			}

			var newCustomer = new Customer
			{
				Email = email,
				Name = email,
				SimpleUserId = newUser.Id,
				RegionId = regionId,
				DistrictId = districtId,
				Address = "",
				PhoneNumber = ""
			};
			_context.Customer.Add(newCustomer);

			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم إضافة الزبون وحساب المستخدم بنجاح.";
			return RedirectToAction(nameof(Index));
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var customer = await _context.Customer
										.Include(c => c.Region)
										.Include(c => c.District)
										.Include(c => c.SimpleUser)
										.FirstOrDefaultAsync(m => m.Id == id);
			if (customer == null)
			{
				return NotFound();
			}
			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", customer.RegionId);
			ViewBag.DistrictId = new SelectList(await _context.District.Where(d => d.RegionId == customer.RegionId).ToListAsync(), "Id", "Name", customer.DistrictId);
			return View(customer);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int id, string email, string newPassword, int regionId, int districtId) // <<<<<<<< المعاملات المعدلة
		{
			var existingCustomer = await _context.Customer
												.Include(c => c.SimpleUser)
												.FirstOrDefaultAsync(c => c.Id == id);

			if (existingCustomer == null)
			{
				return NotFound();
			}

			var existingSimpleUser = existingCustomer.SimpleUser;

			if (existingSimpleUser != null && existingSimpleUser.Email != email)
			{
				var emailExists = await _context.SimpleUsers.AnyAsync(su => su.Email == email && su.Id != existingSimpleUser.Id);
				if (emailExists)
				{
					ModelState.AddModelError("Email", "البريد الإلكتروني هذا مستخدم بالفعل من قبل حساب آخر.");
				}
			}

			ViewBag.RegionId = new SelectList(await _context.Region.ToListAsync(), "Id", "Name", regionId);
			ViewBag.DistrictId = new SelectList(await _context.District.Where(d => d.RegionId == regionId).ToListAsync(), "Id", "Name", districtId);

			if (ModelState.IsValid)
			{
				if (existingSimpleUser != null)
				{
					existingSimpleUser.Email = email;
					if (!string.IsNullOrEmpty(newPassword))
					{
						existingSimpleUser.Password = newPassword;
					}
				}
				else
				{
					ModelState.AddModelError("", "حساب المستخدم المرتبط غير موجود.");
					return View(existingCustomer); 
				}

				try
				{
					existingCustomer.RegionId = regionId;
					existingCustomer.DistrictId = districtId;

					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "تم تعديل بيانات الزبون بنجاح.";
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!CustomerExists(existingCustomer.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
				}
				return RedirectToAction(nameof(Index));
			}
			return View(existingCustomer);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var customer = await _context.Customer
										.Include(c => c.Region)
										.Include(c => c.District)
										.Include(c => c.SimpleUser)
										.FirstOrDefaultAsync(m => m.Id == id);
			if (customer == null)
			{
				return NotFound();
			}

			return View(customer);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var customer = await _context.Customer.FindAsync(id);
			if (customer != null)
			{
				var simpleUserIdToDelete = customer.SimpleUserId;

				_context.Customer.Remove(customer);

				var simpleUser = await _context.SimpleUsers.FindAsync(simpleUserIdToDelete);
				if (simpleUser != null)
				{
					_context.SimpleUsers.Remove(simpleUser);
				}
			}

			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "تم حذف الزبون وحساب المستخدم بنجاح.";
			return RedirectToAction(nameof(Index));
		}

		private bool CustomerExists(int id)
		{
			return _context.Customer.Any(e => e.Id == id);
		}
	}
}