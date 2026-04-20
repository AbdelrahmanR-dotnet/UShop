using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;

namespace UShop.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly UShopDBContext _context;

		public AdminController(UShopDBContext context)
		{
			_context = context;
		}

		// GET: /Admin
		public async Task<IActionResult> Index()
		{
			var admins = await _context.Admins
				.AsNoTracking()
				.ToListAsync();
			return View(admins);
		}

		// GET: /Admin/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var admin = await _context.Admins
				.AsNoTracking()
				.FirstOrDefaultAsync(a => a.Id == id.Value);

			if (admin == null) return NotFound();
			return View(admin);
		}

		// GET: /Admin/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: /Admin/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Admin admin)
		{
			if (!ModelState.IsValid) return View(admin);

			_context.Admins.Add(admin);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}


		// GET: /Admin/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var admin = await _context.Admins.FindAsync(id.Value);
			if (admin == null) return NotFound();
			return View(admin);
		}

		// POST: /Admin/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Admin admin)
		{
			if (id != admin.Id) return NotFound();
			if (!ModelState.IsValid) return View(admin);

			try
			{
				_context.Update(admin);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!AdminExists(admin.Id))
					return NotFound();
				throw;
			}
		}

		// GET: /Admin/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var admin = await _context.Admins
				.AsNoTracking()
				.FirstOrDefaultAsync(a => a.Id == id.Value);

			if (admin == null) return NotFound();
			return View(admin);
		}

		// POST: /Admin/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var admin = await _context.Admins.FindAsync(id);
			if (admin != null)
			{
				_context.Admins.Remove(admin);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		private bool AdminExists(int id)
		{
			return _context.Admins.Any(e => e.Id == id);
		}
	}
}
