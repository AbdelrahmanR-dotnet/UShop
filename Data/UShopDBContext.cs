using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UShop.Models;
namespace UShop.Data;


public class UShopDBContext : IdentityDbContext<User>
{

	public UShopDBContext(DbContextOptions<UShopDBContext> options)
	: base(options)
	{
	}

	public DbSet<Seller> Sellers { get; set; }
	public DbSet<Customer> Customers { get; set; }
	public DbSet<Address> Addresses { get; set; }
	public DbSet<Admin> Admins { get; set; }
	public DbSet<Product> Products { get; set; }
	public DbSet<Category> Categories { get; set; }
	public DbSet<Cart> Carts { get; set; }
	public DbSet<CreditCard> CreditCards { get; set; }
	public DbSet<Order> Orders { get; set; }
	public DbSet<Item> Items { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Item>()
			.HasOne(i => i.Cart)
			.WithMany(c => c.Items)
			.HasForeignKey(i => i.CartId)
			.OnDelete(DeleteBehavior.Cascade); // keep cascade for cart

		modelBuilder.Entity<Item>()
			.HasOne(i => i.Order)
			.WithMany(o => o.Items)
			.HasForeignKey(i => i.OrderId)
			.OnDelete(DeleteBehavior.Restrict); // prevent multiple cascade path
	}


}

