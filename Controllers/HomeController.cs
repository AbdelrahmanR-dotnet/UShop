using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UShop.Data;
using UShop.Models;

public class HomeController : Controller
{
    private readonly UShopDBContext _context;

    public HomeController(UShopDBContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories ?? new List<Category>());
    }
}
