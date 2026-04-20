using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UShop.Models
{
	public class User : IdentityUser
	{
		[Required]
		public UserType UserType { get; set; } // Admin or Customer

		// Navigation properties
		[ForeignKey("Admin")]
		public int? AdminId { get; set; }
		[ValidateNever]
		public Admin? Admin { get; set; }

		[ForeignKey("Customer")]
		public int? CustomerId { get; set; }
		[ValidateNever]
		public Customer? Customer { get; set; }

		[ForeignKey("Seller")]
		public int? SellerId { get; set; }
		[ValidateNever]
		public Seller? Seller { get; set; }
	}
}
