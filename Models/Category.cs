using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UShop.Models
{
	public class Category
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(250)]
		public string? Description { get; set; }

		public string? ImageUrl { get; set; }

		[NotMapped]
		public IFormFile? ImageFile { get; set; }


		public ICollection<Product> Products { get; set; } = new List<Product>();
	}
}
