using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("orderitems")]
    public class OrderItem
    {
        [Key]
        [Column("orderitemid")]
        public int OrderItemId { get; set; }

        [Column("orderid")]
        public int OrderId { get; set; }

        [Column("productid")]
        public int? ProductId { get; set; }

        [Required]
        [Column("productname")]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Column("productsku")]
        [MaxLength(100)]
        public string? ProductSKU { get; set; }

        [Required]
        [Column("quantity")]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column("unitprice", TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column("totalprice", TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
