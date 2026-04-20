using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;

namespace UshopFront.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly UShopDBContext _context;

		public AccountController(
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			UShopDBContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
		}

		// ------------------------------
		// LOGIN
		// ------------------------------
		[AllowAnonymous]
		public IActionResult Login() => View();

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(Login model)
		{
			if (!ModelState.IsValid) return View(model);

			var result = await _signInManager.PasswordSignInAsync(
				model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

			if (result.Succeeded)
			{
				TempData["Message"] = "Login successful!";
				return RedirectToAction("Index", "Home");
			}

			ModelState.AddModelError(string.Empty, "Invalid login credentials.");
			return View(model);
		}

		// ------------------------------
		// REGISTER
		// ------------------------------
		[AllowAnonymous]
		public IActionResult Register() => View();

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(Register model)
		{
			if (!ModelState.IsValid) return View(model);

			var user = new User
			{
				UserName = model.Email,
				Email = model.Email,
				UserType = model.UserType
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				// Assign role based on UserType
				await _userManager.AddToRoleAsync(user, model.UserType.ToString());

				// Create profile entity
				switch (model.UserType)
				{
					case UserType.Admin:
						_context.Admins.Add(new Admin
						{
							FullName = model.FullName,
							Email = model.Email,
							Description = "New admin account",
							User = user
						});
						break;

					case UserType.Customer:
						_context.Customers.Add(new Customer
						{
							FullName = model.FullName,
							Email = model.Email,
							User = user
						});
						break;

					case UserType.Seller:
						_context.Sellers.Add(new Seller
						{
							FullName = model.FullName,
							Email = model.Email,
							User = user
						});
						break;
				}

				await _context.SaveChangesAsync();
				await _signInManager.SignInAsync(user, isPersistent: false);
				TempData["Message"] = "Registration successful!";
				return RedirectToAction("Index", "Home");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error.Description);

			return View(model);
		}

		// ------------------------------
		// PROFILE
		// ------------------------------
		public async Task<IActionResult> Profile()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return RedirectToAction("Login");

			switch (user.UserType)
			{
				case UserType.Admin:
					var admin = await _context.Admins.FirstOrDefaultAsync(a => a.User!.Id == user.Id);
					return View("AdminProfile", admin);

				case UserType.Customer:
					var customer = await _context.Customers
						.Include(c => c.Cart)
						.Include(c => c.Orders)
						.Include(c => c.Address)
						.Include(c => c.CreditCard)
						.FirstOrDefaultAsync(c => c.User!.Id == user.Id);
					return View("CustomerProfile", customer);

				case UserType.Seller:
					var seller = await _context.Sellers
						.Include(s => s.Products)
						.FirstOrDefaultAsync(s => s.User!.Id == user.Id);
					return View("SellerProfile", seller);

				default:
					return NotFound("Profile not found");
			}
		}

		[HttpGet]
		public async Task<IActionResult> EditProfile()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return RedirectToAction("Login");

			return user.UserType switch
			{
				UserType.Admin => View("EditAdminProfile", await _context.Admins.FirstOrDefaultAsync(a => a.User!.Id == user.Id)),
				UserType.Customer => View("EditCustomerProfile", await _context.Customers.FirstOrDefaultAsync(c => c.User!.Id == user.Id)),
				UserType.Seller => View("EditSellerProfile", await _context.Sellers.FirstOrDefaultAsync(s => s.User!.Id == user.Id)),
				_ => NotFound("Profile not found")
			};
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditAdminProfile(Admin model)
		{
			if (!ModelState.IsValid) return View(model);
			_context.Admins.Update(model);
			await _context.SaveChangesAsync();
			return RedirectToAction("Profile");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditCustomerProfile(Customer model)
		{
			if (!ModelState.IsValid) return View(model);
			_context.Customers.Update(model);
			await _context.SaveChangesAsync();
			return RedirectToAction("Profile");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditSellerProfile(Seller model)
		{
			if (!ModelState.IsValid) return View(model);
			_context.Sellers.Update(model);
			await _context.SaveChangesAsync();
			return RedirectToAction("Profile");
		}

		// ------------------------------
		// LOGOUT
		// ------------------------------
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			TempData["Message"] = "You have been logged out.";
			return RedirectToAction("Login");
		}

		[HttpGet]
		public async Task<IActionResult> LogoutGet()
		{
			await _signInManager.SignOutAsync();
			TempData["Message"] = "You have been logged out.";
			return RedirectToAction("Login");
		}

		// ------------------------------
		// NEW FUNCTIONS TO VIEW PROFILE BY TYPE
		// ------------------------------
		public async Task<IActionResult> ViewProfileCustomer(int id)
		{
			var customer = await _context.Customers
				.Include(c => c.Cart)
				.Include(c => c.Orders)
				.Include(c => c.Address)
				.Include(c => c.CreditCard)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (customer == null) return NotFound();
			return View("CustomerProfile", customer);
		}

		public async Task<IActionResult> ViewProfileSeller(int id)
		{
			var seller = await _context.Sellers
				.Include(s => s.Products)
				.FirstOrDefaultAsync(s => s.Id == id);

			if (seller == null) return NotFound();
			return View("SellerProfile", seller);
		}

		public async Task<IActionResult> ViewProfileAdmin(int id)
		{
			var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == id);
			if (admin == null) return NotFound();
			return View("AdminProfile", admin);
		}
	}
}
