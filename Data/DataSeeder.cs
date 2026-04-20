using Microsoft.AspNetCore.Identity;

namespace UShop.Data;

public static class DataSeeder
{
	public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
	{
		string[] roles = { "Admin", "Seller", "Customer" };

		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				await roleManager.CreateAsync(new IdentityRole(role));
			}
		}
	}
}
