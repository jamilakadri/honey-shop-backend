using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("notificationid")]
        public int NotificationId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("type")]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("isread")]
        public bool IsRead { get; set; } = false;

        [Column("relatedentitytype")]
        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }

        [Column("relatedentityid")]
        public int? RelatedEntityId { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
