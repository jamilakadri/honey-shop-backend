using System.ComponentModel.DataAnnotations;

namespace MielShop.API.DTOs.Review
{
    public class CreateReviewDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "La note doit être entre 1 et 5")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    
}