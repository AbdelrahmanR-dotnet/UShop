using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UShop.Models
{
	public class CreditCard
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Card number is required")]
		public string CardNumber { get; set; }

		[Required(ErrorMessage = "Cardholder name is required")]
		[StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be 2–50 characters")]
		public string CardholderName { get; set; }

		[Required(ErrorMessage = "Expiry month is required")]
		[Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
		public int ExpiryMonth { get; set; }

		[Required(ErrorMessage = "Expiry year is required")]
		[Range(2025, 2050, ErrorMessage = "Expiry year must be between 2025 and 2050")]
		public int ExpiryYear { get; set; }

		[Required(ErrorMessage = "CVV is required")]
		[RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
		public string CVV { get; set; }

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
