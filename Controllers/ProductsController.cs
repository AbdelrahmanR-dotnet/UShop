using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;
using UShop.Models.ViewModels;

namespace UShop.Controllers
{
	public class ProductsController : Controller
	{
		private readonly UShopDBContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly UserManager<User> _userManager;


		public ProductsController(UShopDBContext context, IWebHostEnvironment webHostEnvironment, UserManager<User> userManager)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
			_userManager = userManager;
		}

		// GET: Product
		[Authorize(Roles = Roles.Admin + "," + Roles.Seller)]
		public async Task<IActionResult> Index(string searchString, int? categoryId)
		{
			try
			{
				// Get current user Id (ASP.NET Identity style)
				var currentUser = await _userManager.GetUserAsync(User);
				var sellerId = currentUser.SellerId;

				// Only get products belonging to this seller
				var products = _context.Products
					.Include(p => p.Category)
					.Include(p => p.Seller)
					.AsQueryable();

				if (currentUser.UserType == UserType.Seller)
				{
					products = products.Where(p => p.SellerId == sellerId).AsQueryable(); // filter by seller
				}

				if (!string.IsNullOrEmpty(searchString))
				{
					products = products.Where(p => p.Name.Contains(searchString));
					ViewData["CurrentFilter"] = searchString;
				}

				if (categoryId.HasValue && categoryId.Value > 0)
				{
					products = products.Where(p => p.CategoryId == categoryId);
					ViewData["CurrentCategory"] = categoryId;
				}

				// Setup category list for filter
				ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");

				return View(await products.ToListAsync());
			}
			catch (Exception ex)
			{
				// Log the error
				ViewBag.ErrorMessage = $"An error occurred while loading products: {ex.Message}";
				return View(new List<Product>());
			}
		}

		public async Task<IActionResult> Shopping(string searchString, int? categoryId)
		{
			try
			{
				var products = _context.Products.Include(p => p.Category).Include(p => p.Seller).AsQueryable();

				if (!string.IsNullOrEmpty(searchString))
				{
					products = products.Where(p => p.Name.Contains(searchString));
					ViewData["CurrentFilter"] = searchString;
				}

				if (categoryId.HasValue && categoryId.Value > 0)
				{
					products = products.Where(p => p.CategoryId == categoryId);
					ViewData["CurrentCategory"] = categoryId;
				}
				ViewBag.SearchString = searchString; // keep value for input

				// Setup category list for filter
				ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");

				return View(await products.ToListAsync());
			}
			catch (Exception ex)
			{
				// Log the error
				ViewBag.ErrorMessage = $"An error occurred while loading products: {ex.Message}";
				return View(new List<Product>());
			}
		}

		// GET: Product/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			try
			{
				var product = await _context.Products
					.Include(p => p.Category)
					.Include(p => p.Seller)
					.FirstOrDefaultAsync(m => m.Id == id);

				if (product == null)
				{
					return NotFound();
				}

				return View(product);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"An error occurred while loading product details: {ex.Message}";
				return View("Error");
			}
		}

		// GET: Product/Create
		public IActionResult Create()
		{
			// Create a new instance of the ViewModel
			ProductVM productVM = new()
			{
				// Populate the CategoryList from the database
				CategoryList = _context.Categories.Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),

				// Populate the SellerList from the database
				SellerList = _context.Sellers.Select(u => new SelectListItem
				{
					Text = u.FullName,
					Value = u.Id.ToString()
				})
			};
			// Send the ViewModel to the View
			return View(productVM);
		}
		// POST: Product/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductVM productVM)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var product = new Product
					{
						Name = productVM.Name,
						Description = productVM.Description,
						Price = productVM.Price,
						StockQuantity = productVM.StockQuantity,
						CategoryId = productVM.CategoryId,
						SellerId = productVM.SellerId
					};

					// Handle image upload
					if (productVM.ImageFile != null)
					{
						product.ImageUrl = await SaveImage(productVM.ImageFile);
					}

					_context.Add(product);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}

				// Reload lists if there's an error
				productVM.CategoryList = await GetCategoryList();
				productVM.SellerList = await GetSellerList();
				return View(productVM);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"An error occurred while creating the product: {ex.Message}";
				productVM.CategoryList = await GetCategoryList();
				productVM.SellerList = await GetSellerList();
				return View(productVM);
			}
		}

		// GET: Product/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			try
			{
				var product = await _context.Products.FindAsync(id);
				if (product == null)
				{
					return NotFound();
				}

				ProductVM productVM = new()
				{
					Id = product.Id,
					Name = product.Name,
					Description = product.Description,
					Price = product.Price,
					StockQuantity = product.StockQuantity,
					ImageUrl = product.ImageUrl,
					CategoryId = product.CategoryId,
					SellerId = product.SellerId,
					CategoryList = await GetCategoryList(),
					SellerList = await GetSellerList()
				};

				return View(productVM);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"An error occurred while loading edit page: {ex.Message}";
				return View("Error");
			}
		}

		// POST: Product/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ProductVM productVM)
		{
			if (id != productVM.Id)
			{
				return NotFound();
			}

			try
			{
				if (ModelState.IsValid)
				{
					var product = await _context.Products.FindAsync(id);
					if (product == null)
					{
						return NotFound();
					}

					// Update fields
					product.Name = productVM.Name;
					product.Description = productVM.Description;
					product.Price = productVM.Price;
					product.StockQuantity = productVM.StockQuantity;
					product.CategoryId = productVM.CategoryId;
					product.SellerId = productVM.SellerId;

					// Handle image upload
					if (productVM.ImageFile != null)
					{
						// delete old image if exists
						if (!string.IsNullOrEmpty(product.ImageUrl))
						{
							DeleteImage(product.ImageUrl);
						}

						product.ImageUrl = await SaveImage(productVM.ImageFile);
					}

					_context.Update(product);
					await _context.SaveChangesAsync();

					return RedirectToAction(nameof(Index));
				}

				// repopulate lists if validation fails
				productVM.CategoryList = await GetCategoryList();
				productVM.SellerList = await GetSellerList();
				return View(productVM);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ProductExists(productVM.Id))
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
				ViewBag.ErrorMessage = $"An error occurred while updating the product: {ex.Message}";
				productVM.CategoryList = await GetCategoryList();
				productVM.SellerList = await GetSellerList();
				return View(productVM);
			}
		}


		// GET: Product/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			try
			{
				var product = await _context.Products
					.Include(p => p.Category)
					.Include(p => p.Seller)
					.FirstOrDefaultAsync(m => m.Id == id);

				if (product == null)
				{
					return NotFound();
				}

				return View(product);
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"An error occurred while loading delete page: {ex.Message}";
				return View("Error");
			}
		}

		// POST: Product/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				var product = await _context.Products.FindAsync(id);
				if (product != null)
				{
					// Delete image if exists
					if (!string.IsNullOrEmpty(product.ImageUrl))
					{
						DeleteImage(product.ImageUrl);
					}

					_context.Products.Remove(product);
					await _context.SaveChangesAsync();
				}
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"An error occurred while deleting the product: {ex.Message}";
				return RedirectToAction(nameof(Index));
			}
		}

		// Helper Methods
		private bool ProductExists(int id)
		{
			return _context.Products.Any(e => e.Id == id);
		}

		private async Task<IEnumerable<SelectListItem>> GetCategoryList()
		{
			var categories = await _context.Categories.ToListAsync();
			return categories.Select(c => new SelectListItem
			{
				Value = c.Id.ToString(),
				Text = c.Name
			});
		}

		private async Task<IEnumerable<SelectListItem>> GetSellerList()
		{
			var sellers = await _context.Sellers.ToListAsync();
			return sellers.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.FullName
			});
		}

		private async Task<string> SaveImage(IFormFile imageFile)
		{
			var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
			if (!Directory.Exists(uploadsFolder))
			{
				Directory.CreateDirectory(uploadsFolder);
			}

			var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
			var filePath = Path.Combine(uploadsFolder, uniqueFileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await imageFile.CopyToAsync(stream);
			}

			return "/images/products/" + uniqueFileName;
		}

		private void DeleteImage(string imageUrl)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}
			}
		}

	}
}