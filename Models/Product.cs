using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("products")]
    public class Product{
        [Key]
        [Column("productid")]
        public int ProductId { get; set; }

        [Column("categoryid")]
        public int? CategoryId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("slug")]
        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("shortdescription")]
        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        [Required]
        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column("compareatprice", TypeName = "decimal(10,2)")]
        public decimal? CompareAtPrice { get; set; }

        [Column("cost", TypeName = "decimal(10,2)")]
        public decimal? Cost { get; set; }

        [Column("stockquantity")]
        public int StockQuantity { get; set; } = 0;

        [Column("lowstockthreshold")]
        public int LowStockThreshold { get; set; } = 10;

        [Column("sku")]
        [MaxLength(100)]
        public string? SKU { get; set; }

        [Column("weight", TypeName = "decimal(10,2)")]
        public decimal? Weight { get; set; }

        [Column("origin")]
        [MaxLength(100)]
        public string? Origin { get; set; }

        [Column("harvestdate")]
        public DateTime? HarvestDate { get; set; }

        [Column("expirydate")]
        public DateTime? ExpiryDate { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("isfeatured")]
        public bool IsFeatured { get; set; } = false;

        [Column("viewcount")]
        public int ViewCount { get; set; } = 0;

        [Column("salecount")]
        public int SaleCount { get; set; } = 0;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedat")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    }
}