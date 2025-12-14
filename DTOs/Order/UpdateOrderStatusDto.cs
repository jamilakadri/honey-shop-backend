using System.ComponentModel.DataAnnotations;

namespace MielShop.API.DTOs.Order
{


    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}