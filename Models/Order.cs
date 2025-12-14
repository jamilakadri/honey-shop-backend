using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("orderid")]
        public int OrderId { get; set; }

        [Required]
        [Column("ordernumber")]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Column("userid")]
        public int? UserId { get; set; }

        // Facturation
        [Column("billingaddressid")]
        public int? BillingAddressId { get; set; }

        [Required]
        [Column("billingfullname")]
        [MaxLength(200)]
        public string BillingFullName { get; set; } = string.Empty;

        [Required]
        [Column("billingemail")]
        [MaxLength(255)]
        public string BillingEmail { get; set; } = string.Empty;

        [Required]
        [Column("billingphone")]
        [MaxLength(20)]
        public string BillingPhone { get; set; } = string.Empty;

        [Required]
        [Column("billingaddress")]
        public string BillingAddress { get; set; } = string.Empty;

        // Livraison
        [Column("shippingaddressid")]
        public int? ShippingAddressId { get; set; }

        [Required]
        [Column("shippingfullname")]
        [MaxLength(200)]
        public string ShippingFullName { get; set; } = string.Empty;

        [Required]
        [Column("shippingphone")]
        [MaxLength(20)]
        public string ShippingPhone { get; set; } = string.Empty;

        [Required]
        [Column("shippingaddress")]
        public string ShippingAddress { get; set; } = string.Empty;

        // Montants
        [Required]
        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }

        [Column("shippingcost", TypeName = "decimal(10,2)")]
        public decimal ShippingCost { get; set; } = 0;

        [Column("tax", TypeName = "decimal(10,2)")]
        public decimal Tax { get; set; } = 0;

        [Column("discountamount", TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Column("totalamount", TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        // Statuts
        [Column("orderstatus")]
        [MaxLength(50)]
        public string OrderStatus { get; set; } = "Pending";

        [Column("paymentstatus")]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("adminnotes")]
        public string? AdminNotes { get; set; }

        [Column("trackingnumber")]
        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [Column("shippingprovider")]
        [MaxLength(100)]
        public string? ShippingProvider { get; set; }

        // Dates
        [Column("orderdate")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column("paidat")]
        public DateTime? PaidAt { get; set; }

        [Column("shippedat")]
        public DateTime? ShippedAt { get; set; }

        [Column("deliveredat")]
        public DateTime? DeliveredAt { get; set; }

        [Column("cancelledat")]
        public DateTime? CancelledAt { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedat")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}