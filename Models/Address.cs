using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("addresses")]
    public class Address
    {
        [Key]
        [Column("addressid")]
        public int AddressId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("addresstype")]
        [MaxLength(20)]
        public string AddressType { get; set; } = string.Empty;

        [Required]
        [Column("fullname")]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Column("addressline1")]
        [MaxLength(255)]
        public string AddressLine1 { get; set; } = string.Empty;

        [Column("addressline2")]
        [MaxLength(255)]
        public string? AddressLine2 { get; set; }

        [Required]
        [Column("city")]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Column("state")]
        [MaxLength(100)]
        public string? State { get; set; }

        [Required]
        [Column("postalcode")]
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Column("country")]
        [MaxLength(100)]
        public string Country { get; set; } = "Tunisie";

        [Required]
        [Column("phonenumber")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("isdefault")]
        public bool IsDefault { get; set; } = false;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}