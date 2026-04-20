using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UShop.Models
{
	public class Order
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int CustomerId { get; set; }

		[ValidateNever]
		public Customer? Customer { get; set; }

		[Required]
		public DateTime OrderDate { get; set; } = DateTime.UtcNow;

		public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
		public ICollection<Seller> Sellers { get; set; } = new List<Seller>();
		public IList<OrderStatus> Statuses { get; set; } = new List<OrderStatus>();
		public ICollection<Item> Items { get; set; } = new List<Item>();

		// e.g., Pending, Processing, Shipped, Delivered
		[Required]
		public OrderStatus Status { get; set; } = OrderStatus.Pending;

		[NotMapped]
		public OrderStatus CurrentStatus
		{
			get
			{
				if (Statuses.Any() && Statuses.All(s => s == Statuses.First()))
				{
					// All statuses in the collection match—use that
					return Statuses.First();
				}

				// Otherwise keep whatever is already saved in CurrentStatus
				return Status;
			}
			set
			{
				// Optionally allow setting it, which also updates CurrentStatus
				Status = value;
			}
		}

		[NotMapped]
		public decimal TotalAmount => Items.Sum(item => item.Quantity * item.UnitPrice);
	}
}
