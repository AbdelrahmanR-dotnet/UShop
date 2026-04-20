using System.ComponentModel.DataAnnotations;

namespace UShop.Models
{
	public class Register
	{
		public string FullName { get; set; } = string.Empty;

		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public UserType UserType { get; set; } // Admin or Customer

		[Required]
		[StringLength(100, MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;

		[DataType(DataType.Password)]
		[Compare("Password")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
