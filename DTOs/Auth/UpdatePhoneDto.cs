using System.ComponentModel.DataAnnotations;

namespace MielShop.API.DTOs.Auth
{
    public class UpdatePhoneDto
    {
        [Phone(ErrorMessage = "Le format du numéro de téléphone n'est pas valide")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "Le numéro de téléphone doit contenir entre 8 et 15 caractères")]
        public string PhoneNumber { get; set; }
    }
}