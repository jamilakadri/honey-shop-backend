using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("promocodeusage")]
    public class PromoCodeUsage
    {
        [Key]
        [Column("usageid")]
        public int UsageId { get; set; }

        [Column("promocodeid")]
        public int PromoCodeId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("orderid")]
        public int OrderId { get; set; }

        [Required]
        [Column("discountamount", TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }

        [Column("usedat")]
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("PromoCodeId")]
        public virtual PromoCode PromoCode { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}