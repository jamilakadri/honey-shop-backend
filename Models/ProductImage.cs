using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
	[Table("productimages")]
	public class ProductImage
	{
		[Key]
		[Column("imageid")]
		public int ImageId { get; set; }

		[Column("productid")]
		public int ProductId { get; set; }

		[Required]
		[Column("imageurl")]
		[MaxLength(500)]
		public string ImageUrl { get; set; } = string.Empty;

		[Column("alttext")]
		[MaxLength(255)]
		public string? AltText { get; set; }

		[Column("displayorder")]
		public int DisplayOrder { get; set; } = 0;

		[Column("isprimary")]
		public bool IsPrimary { get; set; } = false;

		[Column("createdat")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Relations
		[ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}