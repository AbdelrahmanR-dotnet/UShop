using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;

namespace UShop.Controllers
{
	public class OrdersController : Controller
	{
		private readonly UShopDBContext _context;
		private readonly UserManager<User> _userManager;


		public OrdersController(UShopDBContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;

		}
		private async Task<int?> GetCustomerIdAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			return user?.CustomerId;
		}
		// GET: Orders
		public async Task<IActionResult> Index()
		{
			var customerId = await GetCustomerIdAsync();
			if (customerId == null)
				return Unauthorized();

			var orders = await _context.Orders
				 .Where(o => o.CustomerId == customerId) // only logged-in user's orders
				 .Include(o => o.Customer)
				 .Include(o => o.Items)
					  .ThenInclude(oi => oi.Product)
				 .ToListAsync();

			return View(orders);
		}

		// GET: Orders/Details/5
		public async Task<IActionResult> Details(int id)
		{
			var customerId = await GetCustomerIdAsync();
			if (customerId == null)
				return Unauthorized(); // TODO: replace with logged-in user ID

			var order = await _context.Orders
				 .Where(o => o.CustomerId == customerId) // secure: user can only access own order
				 .Include(o => o.Customer)
				 .Include(o => o.Items)
					  .ThenInclude(oi => oi.Product)
				 .FirstOrDefaultAsync(o => o.Id == id);

			if (order == null) return NotFound();

			return View(order);
		}

		// GET: Orders/Create
		public IActionResult Create()
		{
			// Customers dropdown
			ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "FullName");
			// Products dropdown (for order items)
			ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");

			return View();
		}

		// POST: Orders/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Order order, List<int> productIds, List<int> quantities)
		{
			if (ModelState.IsValid)
			{
				// Add order
				_context.Orders.Add(order);
				await _context.SaveChangesAsync();

				// Add order items
				if (productIds != null && quantities != null && productIds.Count == quantities.Count)
				{
					for (int i = 0; i < productIds.Count; i++)
					{
						if (quantities[i] <= 0) // Add validation
						{
							ModelState.AddModelError("", $"Quantity must be greater than 0");
							continue;
						}

						var product = await _context.Products.FindAsync(productIds[i]);
						if (product != null)
						{
							var orderItem = new Item
							{
								OrderId = order.Id,
								ProductId = product.Id,
								Quantity = quantities[i],
								UnitPrice = product.Price
							};
							_context.Items.Add(orderItem);
						}
					}
					await _context.SaveChangesAsync();
				}

				return RedirectToAction(nameof(Index));
			}

			// If invalid, reload dropdowns
			ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "FullName", order.CustomerId);
			ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");

			return View(order);
		}

		// GET: Orders/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			var order = await _context.Orders
				  .Include(o => o.Items)
				  .FirstOrDefaultAsync(o => o.Id == id);

			if (order == null) return NotFound();

			ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "FullName", order.CustomerId);

			return View(order);
		}

		// POST: Orders/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Order order)
		{
			if (id != order.Id) return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(order);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Orders.Any(o => o.Id == order.Id))
						return NotFound();
					throw;
				}
				return RedirectToAction(nameof(Index));
			}

			ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "FullName", order.CustomerId);
			return View(order);
		}

		// POST: Orders/UpdateStatus - Admin only method
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
		{
			var order = await _context.Orders.FindAsync(id);
			if (order == null) return NotFound();

			// Validate status transitions (optional business logic)
			if (!IsValidStatusTransition(order.Status, newStatus))
			{
				TempData["Error"] = "Invalid status transition";
				return RedirectToAction(nameof(Details), new { id });
			}

			order.Status = newStatus;
			await _context.SaveChangesAsync();

			TempData["Success"] = $"Order status updated to {newStatus}";
			return RedirectToAction(nameof(Details), new { id });
		}

		// Helper method for status validation
		private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
		{
			// Define valid status transitions
			return currentStatus switch
			{
				OrderStatus.Pending => newStatus == OrderStatus.Processing || newStatus == OrderStatus.Cancelled,
				OrderStatus.Processing => newStatus == OrderStatus.Shipped || newStatus == OrderStatus.Cancelled,
				OrderStatus.Shipped => newStatus == OrderStatus.Delivered,
				OrderStatus.Delivered => false, // No transitions from delivered
				OrderStatus.Cancelled => false, // No transitions from cancelled
				_ => false
			};
		}

		// GET: Orders/Delete/5
		public async Task<IActionResult> Delete(int id)
		{
			var order = await _context.Orders
				  .Include(o => o.Customer)
				  .FirstOrDefaultAsync(m => m.Id == id);

			if (order == null) return NotFound();

			return View(order);
		}

		// POST: Orders/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var order = await _context.Orders
				  .Include(o => o.Items)
				  .FirstOrDefaultAsync(o => o.Id == id);

			if (order != null)
			{
				// Delete related order items first
				_context.Items.RemoveRange(order.Items);
				_context.Orders.Remove(order);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));
		}

		// POST: Orders/Cancel
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Cancel(int id)
		{
			var order = await _context.Orders
				 .Include(o => o.Customer)
				 .FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			// Check if the order can be cancelled
			if (!CanOrderBeCancelled(order.Status))
			{
				TempData["Error"] = "This order cannot be cancelled. It may have already been shipped or delivered.";
				return RedirectToAction(nameof(Details), new { id });
			}

			// Update order status to cancelled
			order.Status = OrderStatus.Cancelled;

			try
			{
				await _context.SaveChangesAsync();
				TempData["Success"] = "Order has been cancelled successfully.";
			}
			catch (Exception ex)
			{
				TempData["Error"] = "An error occurred while cancelling the order. Please try again.";
			}

			return RedirectToAction(nameof(Details), new { id });
		}

		// Helper method to check if order can be cancelled
		private bool CanOrderBeCancelled(OrderStatus status)
		{
			// Only allow cancellation for Pending and Processing orders
			return status == OrderStatus.Pending || status == OrderStatus.Processing;
		}

		// Optional: GET method to show cancellation confirmation page
		[HttpGet]
		public async Task<IActionResult> CancelConfirmation(int id)
		{
			var order = await _context.Orders
				 .Include(o => o.Customer)
				 .Include(o => o.Items)
					  .ThenInclude(oi => oi.Product)
				 .FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			if (!CanOrderBeCancelled(order.Status))
			{
				TempData["Error"] = "This order cannot be cancelled.";
				return RedirectToAction(nameof(Details), new { id });
			}

			return View(order);
		}
	}
}