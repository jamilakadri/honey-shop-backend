using System.ComponentModel.DataAnnotations;

namespace MielShop.API.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public int ShippingAddressId { get; set; }

        [Required]
        public int BillingAddressId { get; set; }

        public string? PromoCode { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

}