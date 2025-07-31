using Microsoft.AspNetCore.Mvc;
using WebApplication3.Data;
using WebApplication3.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace WebApplication33.Controllers
{
	public class AccountController : Controller
	{
		private readonly ApplicationDbContext _context;

		public AccountController(ApplicationDbContext context)
		{
			_context = context;
		}

		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(string email, string password, string selectedRole)
		{
			var user = await _context.SimpleUsers.FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.Role == selectedRole);

			if (user == null)
			{
				ViewBag.Error = "بيانات الدخول غير صحيحة أو نوع الحساب لا يطابق.";
				return View();
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.Email),
				new Claim(ClaimTypes.Role, user.Role)
			};

			if (selectedRole == "Customer")
			{
				var customer = await _context.Customer.FirstOrDefaultAsync(c => c.SimpleUserId == user.Id);
				if (customer != null)
				{
					claims.Add(new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString())); 
				}
				else
				{
					ViewBag.Error = "بيانات العميل المرتبطة غير مكتملة. يرجى الاتصال بالدعم.";
					return View();
				}
			}
			else 
			{
				claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
			}

			var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

			return user.Role switch
			{
				"Admin" => RedirectToAction("Dashboard", "Admins"),
				"Distributor" => RedirectToAction("Dashboard", "Distributors"),
				_ => RedirectToAction("Dashboard", "Customers")
			};
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			return RedirectToAction("Login");
		}

		public async Task<IActionResult> Register()
		{
			ViewBag.Regions = new SelectList(await _context.Region.ToListAsync(), "Id", "Name");
			ViewBag.Districts = new SelectList(new List<District>(), "Id", "Name");
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(string email, string password, string selectedRole, int? regionId, int? districtId)
		{
			ViewBag.Regions = new SelectList(await _context.Region.ToListAsync(), "Id", "Name");
			ViewBag.Districts = new SelectList(await _context.District.ToListAsync(), "Id", "Name");
			if (selectedRole != "Customer" && selectedRole != "Distributor")
			{
				ViewBag.Error = "يُسمح فقط بتسجيل الزبائن أو الموزعين.";
				return View();
			}

			var userExists = await _context.SimpleUsers.AnyAsync(u => u.Email == email);
			if (userExists)
			{
				ViewBag.Error = "البريد الإلكتروني مستخدم مسبقًا.";
				return View();
			}

			var newUser = new SimpleUser
			{
				Email = email,
				Password = password,
				Role = selectedRole
			};
			_context.SimpleUsers.Add(newUser);
			await _context.SaveChangesAsync(); 

			if (selectedRole == "Customer")
			{
				if (!regionId.HasValue || !districtId.HasValue)
				{
					ViewBag.Error = "الرجاء اختيار المنطقة والمنطقة الفرعية.";
					_context.SimpleUsers.Remove(newUser);
					await _context.SaveChangesAsync();
					return View();
				}

				var newCustomer = new Customer
				{
					Email = email,
					Name = email,
					SimpleUserId = newUser.Id,
					RegionId = regionId.Value, 
					DistrictId = districtId.Value, 
					Address = "",      
					PhoneNumber = ""   
				};
				_context.Customer.Add(newCustomer);
				await _context.SaveChangesAsync();
			}

			ViewBag.SuccessMessage = "تم تسجيل حسابك بنجاح. يرجى تسجيل الدخول.";
			return RedirectToAction("Login");
		}
	}
}