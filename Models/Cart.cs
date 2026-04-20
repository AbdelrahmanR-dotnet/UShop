using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UShop.Models
{
	public class Cart
	{
		[Key]
		public int Id { get; set; }

		[Required, ForeignKey("Customer")]  
		public int CustomerId { get; set; }

		[ValidateNever]
		public Customer? Customer { get; set; }

		public ICollection<Item> Items { get; set; } = new List<Item>();

		[NotMapped]
		public decimal TotalAmount => Items.Sum(item => item.Quantity * item.UnitPrice);
	}
}
