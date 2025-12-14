using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("wishlists")]
    public class Wishlist
    {
        [Key]
        [Column("wishlistid")]
        public int WishlistId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("productid")]
        public int ProductId { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}