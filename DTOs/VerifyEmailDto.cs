using System.ComponentModel.DataAnnotations;

namespace MielShop.API.DTOs.Auth
{
    public class VerifyEmailDto
    {
        [Required(ErrorMessage = "Le token est requis")]
        public string Token { get; set; } = string.Empty;
    }

    public class ResendVerificationEmailDto
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;
    }
}