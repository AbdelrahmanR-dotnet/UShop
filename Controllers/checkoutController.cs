using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;

namespace UShop.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly UShopDBContext _context;
    private readonly UserManager<User> _userManager;

    public CheckoutController(UShopDBContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var customer = await _context.Customers
            .Include(c => c.CreditCard)
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c => c.Email == user.Email);

        if (customer == null)
        {
            customer = new Customer { FullName = user.UserName ?? string.Empty, Email = user.Email ?? string.Empty };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        // 1. FETCH THE CART AND ITS ITEMS
        var cart = await _context.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.Id);

        // 2. REDIRECT IF CART IS EMPTY
        if (cart == null || !cart.Items.Any())
        {
            TempData["Error"] = "Your cart is empty. Add items before checking out.";
            return RedirectToAction("Index", "Cart");
        }

        var order = new Order
        {
            Customer = customer,
            // 3. ASSIGN THE CART ITEMS TO THE ORDER
            Items = cart.Items.ToList(),
            PaymentMethod = PaymentMethod.CashOnDelivery
        };

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Place(Order order, CreditCard creditCard, bool? UseSavedCard)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var customer = await _context.Customers
            .Include(c => c.CreditCard)
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c => c.Email == user.Email);

        // Fetch the cart again for the POST action
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customer!.Id);

        if (cart == null || !cart.Items.Any())
            return RedirectToAction("Index", "Cart");

        // Validate Profile
        if (string.IsNullOrEmpty(customer!.FullName) || string.IsNullOrEmpty(customer.Email) ||
            customer.Address == null)
        {
            TempData["Error"] = "Please complete your address information before checkout.";
            return RedirectToAction("Edit", "Customer", new { id = customer.Id });
        }

        // Payment Logic
        if (order.PaymentMethod == PaymentMethod.CreditCard)
        {
            if (UseSavedCard == true && customer.CreditCard != null)
            {
                order.Customer.CreditCard = customer.CreditCard;
            }
            else
            {
                if (!TryValidateModel(creditCard))
                {
                    ModelState.AddModelError(string.Empty, "Invalid credit card details.");
                    order.Items = cart.Items.ToList(); // reload items for the view
                    return View("Index", order);
                }

                creditCard.CustomerId = customer.Id;
                _context.CreditCards.Add(creditCard);
                customer.CreditCard = creditCard;
            }
        }

        // Map order properties
        order.CustomerId = customer.Id;
        order.Customer = customer;
        order.OrderDate = DateTime.UtcNow;
        order.Status = OrderStatus.Pending;

        // 4. TRANSFER ITEMS FROM CART TO ORDER
        order.Items = cart.Items.Select(i => new Item
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList();

        _context.Orders.Add(order);

        // 5. CLEAR THE CART AFTER CHECKOUT
        _context.Items.RemoveRange(cart.Items);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Your order has been placed successfully!";
        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }
}