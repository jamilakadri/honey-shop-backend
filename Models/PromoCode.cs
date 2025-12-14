using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("promocodes")]
    public class PromoCode
    {
        [Key]
        [Column("promocodeid")]
        public int PromoCodeId { get; set; }

        [Required]
        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        [Column("discounttype")]
        [MaxLength(20)]
        public string DiscountType { get; set; } = string.Empty;

        [Required]
        [Column("discountvalue", TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Column("minimumorderamount", TypeName = "decimal(10,2)")]
        public decimal? MinimumOrderAmount { get; set; }

        [Column("maxdiscountamount", TypeName = "decimal(10,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        [Column("usagelimit")]
        public int? UsageLimit { get; set; }

        [Column("usagecount")]
        public int UsageCount { get; set; } = 0;

        [Column("peruserlimit")]
        public int PerUserLimit { get; set; } = 1;

        [Required]
        [Column("startdate")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("enddate")]
        public DateTime EndDate { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public virtual ICollection<PromoCodeUsage> PromoCodeUsages { get; set; } = new List<PromoCodeUsage>();
    }
}
