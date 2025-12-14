using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
	[Table("cartitems")]
	public class CartItem
	{
		[Key]
		[Column("cartitemid")]
		public int CartItemId { get; set; }

		[Column("cartid")]
		public int CartId { get; set; }

		[Column("productid")]
		public int ProductId { get; set; }

		[Required]
		[Column("quantity")]
		[Range(1, int.MaxValue)]
		public int Quantity { get; set; }

		[Required]
		[Column("price", TypeName = "decimal(10,2)")]
		public decimal Price { get; set; }

		[Column("createdat")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[Column("updatedat")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Relations
		[ForeignKey("CartId")]
		public virtual Cart Cart { get; set; } = null!;

		[ForeignKey("ProductId")]
		public virtual Product Product { get; set; } = null!;
	}
}