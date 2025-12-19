// AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MielShop.API.Services;
using MielShop.API.DTOs.Auth;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                if (result == null)
                {
                    return BadRequest(new { message = "Cet email est déjà utilisé" });
                }

                return Ok(new
                {
                    message = "Inscription réussie! Veuillez vérifier votre email.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription");
                return StatusCode(500, new { message = "Erreur serveur lors de l'inscription" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect" });
                }

                return Ok(new { data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Email non vérifié
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion");
                return StatusCode(500, new { message = "Erreur serveur lors de la connexion" });
            }
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Token))
                {
                    return BadRequest(new { message = "Token de vérification manquant" });
                }

                var result = await _authService.VerifyEmailAsync(dto.Token);

                if (!result)
                {
                    return BadRequest(new { message = "Le token est invalide ou a expiré" });
                }

                return Ok(new { message = "Email vérifié avec succès!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification d'email");
                return StatusCode(500, new { message = "Erreur serveur lors de la vérification" });
            }
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous] // ✅ IMPORTANT: Doit être public
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
        {
            try
            {
                _logger.LogInformation($"📧 Resend verification request for: {dto.Email}");

                // ✅ Validation de l'email
                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    return BadRequest(new { message = "Adresse email requise" });
                }

                if (!IsValidEmail(dto.Email))
                {
                    return BadRequest(new { message = "Format d'email invalide" });
                }

                var result = await _authService.ResendVerificationEmailAsync(dto.Email);

                if (!result)
                {
                    // ✅ Message plus clair
                    return BadRequest(new
                    {
                        message = "Impossible de renvoyer l'email. Vérifiez que l'adresse existe et n'est pas déjà vérifiée."
                    });
                }

                return Ok(new
                {
                    message = "Email de vérification renvoyé avec succès! Vérifiez votre boîte de réception."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du renvoi d'email pour {dto.Email}");
                return StatusCode(500, new
                {
                    message = "Erreur serveur lors de l'envoi de l'email"
                });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Utilisateur non authentifié" });
                }

                var result = await _authService.ChangePasswordAsync(userId, dto);

                if (!result)
                {
                    return BadRequest(new { message = "Mot de passe actuel incorrect" });
                }

                return Ok(new { message = "Mot de passe modifié avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe");
                return StatusCode(500, new { message = "Erreur serveur" });
            }
        }

        // ✅ Helper method pour valider l'email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    // ✅ DTOs
    public class VerifyEmailDto
    {
        public string Token { get; set; } = string.Empty;
    }

    public class ResendVerificationDto
    {
        public string Email { get; set; } = string.Empty;
    }
}