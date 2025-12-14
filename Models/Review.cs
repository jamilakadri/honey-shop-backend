using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("reviews")]
    public class Review
    {
        [Key]
        [Column("reviewid")]
        public int ReviewId { get; set; }

        [Column("productid")]
        public int ProductId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("rating")]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Column("title")]
        [MaxLength(200)]
        public string? Title { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("isverifiedpurchase")]
        public bool IsVerifiedPurchase { get; set; } = false;

        [Column("isapproved")]
        public bool IsApproved { get; set; } = false;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
