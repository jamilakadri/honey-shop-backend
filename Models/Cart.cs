using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("carts")]
    public class Cart
    {
        [Key]
        [Column("cartid")]
        public int CartId { get; set; }

        [Column("userid")]
        public int? UserId { get; set; }

        [Column("sessionid")]
        [MaxLength(255)]
        public string? SessionId { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedat")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
