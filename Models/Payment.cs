using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("paymentid")]
        public int PaymentId { get; set; }

        [Column("orderid")]
        public int OrderId { get; set; }

        [Required]
        [Column("paymentmethod")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Column("paymentprovider")]
        [MaxLength(50)]
        public string? PaymentProvider { get; set; }

        [Column("transactionid")]
        [MaxLength(255)]
        public string? TransactionId { get; set; }

        [Required]
        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column("currency")]
        [MaxLength(3)]
        public string Currency { get; set; } = "TND";

        [Required]
        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [Column("paymentdate")]
        public DateTime? PaymentDate { get; set; }

        [Column("refundedamount", TypeName = "decimal(10,2)")]
        public decimal RefundedAmount { get; set; } = 0;

        [Column("refundedat")]
        public DateTime? RefundedAt { get; set; }

        [Column("metadata", TypeName = "jsonb")]
        public string? Metadata { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}