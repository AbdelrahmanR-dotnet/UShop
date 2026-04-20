using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;

namespace UShop.Controllers;

public class CustomerController : Controller
{
    private readonly UShopDBContext _context;

    public CustomerController(UShopDBContext context)
    {
        _context = context;
    }


    public IActionResult Index()
    {
        var customers = _context.Customers
            .Include(c => c.Orders)
            .ToList();
        return View(customers);
    }


    public IActionResult Details(int id)
    {
        var customer = _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Items)
            .FirstOrDefault(c => c.Id == id);

        if (customer == null) return NotFound();

        return View(customer);
    }


    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Customer customer)
    {
        if (ModelState.IsValid)
        {
            _context.Customers.Add(customer);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        return View(customer);
    }

    // GET: Customer/Edit/5
    public IActionResult Edit(int id)
    {
        // FIX: Must include the Address table
        var customer = _context.Customers
            .Include(c => c.Address)
            .FirstOrDefault(c => c.Id == id);

        if (customer == null) return NotFound();

        // Prevent null reference in the view
        if (customer.Address == null) customer.Address = new Address();

        return View(customer);
    }

// POST: Customer/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Customer customer)
    {
        if (id != customer.Id) return BadRequest();

        // Remove Address from ModelState validation temporarily if needed
        // ModelState.Remove("Address.Customer"); 

        if (ModelState.IsValid)
            try
            {
                var existingCustomer = _context.Customers
                    .Include(c => c.Address)
                    .FirstOrDefault(c => c.Id == id);

                if (existingCustomer == null) return NotFound();

                // Update basic info
                existingCustomer.FullName = customer.FullName;
                existingCustomer.PhoneNumber = customer.PhoneNumber;

                // FIX: Manually map the Address fields
                if (existingCustomer.Address == null) existingCustomer.Address = new Address();

                existingCustomer.Address.Street = customer.Address?.Street ?? "";
                existingCustomer.Address.City = customer.Address?.City ?? "";
                existingCustomer.Address.Country = customer.Address?.Country ?? "";

                _context.Update(existingCustomer);
                _context.SaveChanges();

                // Redirect back to checkout so the user can finish their purchase
                return RedirectToAction("Index", "Checkout");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Customers.Any(e => e.Id == id)) return NotFound();
                throw;
            }

        return View(customer);
    }

    public IActionResult Delete(int id)
    {
        var customer = _context.Customers
            .FirstOrDefault(c => c.Id == id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var customer = _context.Customers.Find(id);
        if (customer == null) return NotFound();

        _context.Customers.Remove(customer);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}