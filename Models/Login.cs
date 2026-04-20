using System.ComponentModel.DataAnnotations;

namespace UShop.Models
{
	public class Login
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;
	}
}
