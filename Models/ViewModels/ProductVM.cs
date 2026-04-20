using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UShop.Models.ViewModels
{
	public class ProductVM
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(150)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? Description { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; }

		public int StockQuantity { get; set; }


		[Display(Name = "Product Image")]
		[ValidateNever]
		public IFormFile? ImageFile { get; set; }

		public string? ImageUrl { get; set; }

		[Required]
		public int SellerId { get; set; }

		[ValidateNever]
		public Seller? Seller { get; set; }

		[Required]
		public int CategoryId { get; set; }

		[ValidateNever]
		public Category? Category { get; set; }

		[ValidateNever]
		public IEnumerable<SelectListItem>? CategoryList { get; set; }

		[ValidateNever]
		public IEnumerable<SelectListItem>? SellerList { get; set; }
	}
}


