using System;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace MielShop.API.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("email")]
        [MaxLength(255)]
        public string Email {  get; set; }
        [Required]
        [Column("passwordhash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [Column("firstname")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [Column("lastname")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Column("phonenumber")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Column("role")]
        [MaxLength(20)]
        public string Role { get; set; } = "Customer";

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("emailconfirmed")]
        public bool EmailConfirmed { get; set; } = false;
        [Column("emailverificationtoken")]
        [MaxLength(255)]
        public string? EmailVerificationToken { get; set; }

        [Column("emailverificationtokenexpires")]
        public DateTime? EmailVerificationTokenExpires { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updatedat")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Column("lastlogin")]
        public DateTime? LastLogin { get; set; }

        // Relations (à la fin de la classe User)
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual Cart? Cart { get; set; }

    }
}