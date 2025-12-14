using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MielShop.API.Models
{
    [Table("productattributes")]
    public class ProductAttribute
    {
        [Key]
        [Column("attributeid")]
        public int AttributeId { get; set; }

        [Column("productid")]
        public int ProductId { get; set; }

        [Required]
        [Column("attributename")]
        [MaxLength(100)]
        public string AttributeName { get; set; } = string.Empty;

        [Required]
        [Column("attributevalue")]
        [MaxLength(255)]
        public string AttributeValue { get; set; } = string.Empty;

        [Column("displayorder")]
        public int DisplayOrder { get; set; } = 0;

        // Relations
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}