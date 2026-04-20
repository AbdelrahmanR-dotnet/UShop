using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;
using UshopFront.Controllers;

namespace UShop.Controllers
{
	[Authorize] // Require login
	public class CreditCardController : Controller
	{
		private readonly UShopDBContext _context;
		private readonly UserManager<User> _userManager;

		public CreditCardController(UShopDBContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// Helper: Get logged-in user's owner id (Customer or Seller)
		private async Task<(int? Id, string Type)> GetOwnerIdAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
				return (null, null);

			if (user.UserType == UserType.Customer)
				return (user.CustomerId, "Customer");
			else if (user.UserType == UserType.Seller)
				return (user.SellerId, "Seller");

			return (null, null);
		}

		// GET: CreditCard
		public async Task<IActionResult> Index()
		{
			var (ownerId, ownerType) = await GetOwnerIdAsync();
			if (ownerId == null)
				return Unauthorized();

			if (ownerType == "Customer")
			{
				var customer = await _context.Customers
					.Include(c => c.CreditCard) // include credit card
					.FirstOrDefaultAsync(c => c.Id == ownerId);

				if (customer == null)
					return NotFound();

				return View("Index", customer); // use a dedicated view
			}
			else if (ownerType == "Seller")
			{
				var seller = await _context.Sellers
					.Include(s => s.CreditCard) // include credit card
					.FirstOrDefaultAsync(s => s.Id == ownerId);

				if (seller == null)
					return NotFound();

				return View("Index", seller); // same view works for seller
			}

			return Unauthorized();
		}


		// GET: Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreditCard card)
		{
			var (ownerId, ownerType) = await GetOwnerIdAsync();
			if (ownerId == null)
				return Unauthorized();

			if (!ModelState.IsValid)
				return View(card);

			if (ownerType == "Customer")
				card.CustomerId = ownerId.Value;
			else if (ownerType == "Seller")
				card.SellerId = ownerId.Value;

			_context.CreditCards.Add(card);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(AccountController.Profile));
		}

		// GET: Edit
		public async Task<IActionResult> Edit(int id)
		{
			var (ownerId, ownerType) = await GetOwnerIdAsync();
			if (ownerId == null)
				return Unauthorized();

			var card = await _context.CreditCards
				.FirstOrDefaultAsync(c =>
					(ownerType == "Customer" && c.CustomerId == ownerId && c.Id == id) ||
					(ownerType == "Seller" && c.SellerId == ownerId && c.Id == id)
				);

			if (card == null)
				return NotFound();

			return View(card);
		}

		// POST: Edit
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, CreditCard card)
		{
			var (ownerId, ownerType) = await GetOwnerIdAsync();
			if (ownerId == null)
				return Unauthorized();

			if (id != card.Id)
				return NotFound();

			if (!ModelState.IsValid)
				return View(card);

			var existingCard = await _context.CreditCards
				.FirstOrDefaultAsync(c =>
					(ownerType == "Customer" && c.CustomerId == ownerId && c.Id == id) ||
					(ownerType == "Seller" && c.SellerId == ownerId && c.Id == id)
				);

			if (existingCard == null)
				return NotFound();

			existingCard.CardNumber = card.CardNumber;
			existingCard.CardholderName = card.CardholderName;
			existingCard.ExpiryMonth = card.ExpiryMonth;
			existingCard.ExpiryYear = card.ExpiryYear;
			existingCard.CVV = card.CVV;

			_context.Update(existingCard);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		// GET: Delete
		public async Task<IActionResult> Delete(int id)
		{
			var (ownerId, ownerType) = await GetOwnerIdAsync();
			if (ownerId == null)
				return Unauthorized();

			var card = await _context.CreditCards
				.FirstOrDefaultAsync(c =>
					(ownerType == "Customer" && c.CustomerId == ownerId && c.Id == id) ||
					(ownerType == "Seller" && c.SellerId == ownerId && c.Id == id)
				);

			if (card == null)
				return NotFound();

			return View(card);
		}

		// POST: Delete
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var (ownerId, ownerType) = await GetOwnerIdAsync();
			if (ownerId == null)
				return Unauthorized();

			var card = await _context.CreditCards
				.FirstOrDefaultAsync(c =>
					(ownerType == "Customer" && c.CustomerId == ownerId && c.Id == id) ||
					(ownerType == "Seller" && c.SellerId == ownerId && c.Id == id)
				);

			if (card != null)
			{
				_context.CreditCards.Remove(card);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
