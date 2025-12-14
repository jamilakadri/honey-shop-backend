using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("categories")]
    public class Category
    {
        [Key]
        [Column("categoryid")]
        public int CategoryId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("slug")]
        [MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        [Column("imageurl")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Column("displayorder")]
        public int DisplayOrder { get; set; } = 0;

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}