using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;

namespace UShop.Controllers
{
	public class CategoriesController : Controller
	{
		private readonly UShopDBContext _context;

		public CategoriesController(UShopDBContext context)
		{
			_context = context;
		}

		// Helper: Save uploaded image and return relative path
		private async Task<string> SaveImageAsync(IFormFile? imageFile)
		{
			if (imageFile == null || imageFile.Length == 0)
				return null!;

			var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories");

			if (!Directory.Exists(uploadsFolder))
				Directory.CreateDirectory(uploadsFolder);

			var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
			var filePath = Path.Combine(uploadsFolder, uniqueFileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await imageFile.CopyToAsync(stream);
			}

			return "/images/categories/" + uniqueFileName;
		}

		// GET: Categories
		public async Task<IActionResult> Index(string? search)
		{
			var query = _context.Categories.AsQueryable();

			if (!string.IsNullOrWhiteSpace(search))
			{
				query = query.Where(c => c.Name.Contains(search) ||
										 (c.Description != null && c.Description.Contains(search)));
			}

			var categories = await query.AsNoTracking().ToListAsync();
			ViewBag.Search = search;
			return View(categories);
		}

		// GET: Categories/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Categories/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Category category)
		{
			if (!ModelState.IsValid) return View(category);

			try
			{
				if (category.ImageFile != null)
				{
					category.ImageUrl = await SaveImageAsync(category.ImageFile);
				}

				_context.Add(category);
				await _context.SaveChangesAsync();
				TempData["SuccessMessage"] = "Category created successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error creating category: {ex.Message}");
				return View(category);
			}
		}

		// GET: Categories/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			try
			{
				var category = await _context.Categories
											 .Include(c => c.Products)
											 .AsNoTracking()
											 .FirstOrDefaultAsync(c => c.Id == id);
				if (category == null) return NotFound();

				return View(category);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error loading category details: {ex.Message}");
				return View("Error");
			}
		}

		// GET: Categories/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			try
			{
				var category = await _context.Categories.FindAsync(id);
				if (category == null) return NotFound();

				return View(category);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error loading category for editing: {ex.Message}");
				return View("Error");
			}
		}

		// POST: Categories/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Category category)
		{
			if (id != category.Id) return NotFound();
			if (!ModelState.IsValid) return View(category);

			try
			{
				var existingCategory = await _context.Categories.FindAsync(id);
				if (existingCategory == null) return NotFound();

				existingCategory.Name = category.Name;
				existingCategory.Description = category.Description;

				if (category.ImageFile != null)
				{
					existingCategory.ImageUrl = await SaveImageAsync(category.ImageFile);
				}

				_context.Update(existingCategory);
				await _context.SaveChangesAsync();

				TempData["SuccessMessage"] = "Category updated successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CategoryExists(category.Id)) return NotFound();
				throw;
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error updating category: {ex.Message}");
				return View(category);
			}
		}

		// GET: Categories/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			try
			{
				var category = await _context.Categories
											 .Include(c => c.Products)
											 .AsNoTracking()
											 .FirstOrDefaultAsync(c => c.Id == id);
				if (category == null) return NotFound();

				return View(category);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error loading category for deletion: {ex.Message}");
				return View("Error");
			}
		}

		// POST: Categories/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				var category = await _context.Categories
											 .Include(c => c.Products)
											 .FirstOrDefaultAsync(c => c.Id == id);

				if (category != null)
				{
					if (category.Products.Any())
					{
						TempData["ErrorMessage"] = "Cannot delete category because it contains products. Please delete or reassign the products first.";
						return RedirectToAction(nameof(Delete), new { id });
					}

					_context.Categories.Remove(category);
					await _context.SaveChangesAsync();
					TempData["SuccessMessage"] = "Category deleted successfully!";
				}

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
				return RedirectToAction(nameof(Index));
			}
		}

		private bool CategoryExists(int id)
		{
			return _context.Categories.Any(e => e.Id == id);
		}
	}
}
