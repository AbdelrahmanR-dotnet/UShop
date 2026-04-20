using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace UShop.Models
{
	public class Seller
	{
		[Key]
		public int Id { get; set; }
		[Required, MaxLength(100)]
		public string FullName { get; set; } = string.Empty;
		[Required, MaxLength(100)]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;
		[MaxLength(15)]
		public string? PhoneNumber { get; set; }

		public string? ImageUrl { get; set; }

		// Relationships
		[ValidateNever]
		public User? User { get; set; }

		[ValidateNever]
		public CreditCard? CreditCard { get; set; }

		public ICollection<Product> Products { get; set; } = new List<Product>();
		public ICollection<Order> Orders { get; set; } = new List<Order>();

	}
}
