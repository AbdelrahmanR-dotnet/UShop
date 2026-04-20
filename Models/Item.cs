using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UShop.Models
{
	public class Item
	{
		[Key]
		public int Id { get; set; }

		public int? OrderId { get; set; }
		[ValidateNever]
		public Order? Order { get; set; }

		public int? CartId { get; set; }
		[ValidateNever]
		public Cart? Cart { get; set; }

		[Required]
		public int ProductId { get; set; }
		[ValidateNever]
		public Product? Product { get; set; }

		[Required]
		public int Quantity { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal UnitPrice { get; set; }

		[NotMapped]
		public decimal TotalPrice => Quantity * UnitPrice;
	}
}
