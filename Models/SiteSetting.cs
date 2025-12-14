using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("sitesettings")]
    public class SiteSetting
    {
        [Key]
        [Column("settingid")]
        public int SettingId { get; set; }

        [Required]
        [Column("settingkey")]
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Column("settingvalue")]
        public string? SettingValue { get; set; }

        [Column("description")]
        [MaxLength(255)]
        public string? Description { get; set; }

        [Column("updatedat")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}