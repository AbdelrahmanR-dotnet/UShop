using System.ComponentModel.DataAnnotations.Schema;

namespace UShop.Models
{
	public class Address
	{
		public int Id { get; set; }
		public string Street { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;

		[ForeignKey("Customer")]
		public int CustomerId { get; set; }
		public Customer Customer { get; set; } = null!;

		public override string ToString()
		{
			return $"{Street}, {City}, {Country}";
		}
	}
}
