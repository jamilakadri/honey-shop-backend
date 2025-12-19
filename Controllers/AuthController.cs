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
                _logger.LogInformation($"📝 Registration attempt for: {registerDto.Email}");

                var result = await _authService.RegisterAsync(registerDto);
                if (result == null)
                {
                    _logger.LogWarning($"❌ Registration failed - email already exists: {registerDto.Email}");
                    return BadRequest(new { message = "Cet email est déjà utilisé" });
                }

                _logger.LogInformation($"✅ User registered successfully: {registerDto.Email}");
                return Ok(new
                {
                    message = "Inscription réussie! Veuillez vérifier votre email.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error during registration for {registerDto.Email}");
                return StatusCode(500, new { message = "Erreur serveur lors de l'inscription" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation($"🔐 Login attempt for: {loginDto.Email}");

                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                {
                    _logger.LogWarning($"❌ Login failed - invalid credentials: {loginDto.Email}");
                    return Unauthorized(new { message = "Email ou mot de passe incorrect" });
                }

                _logger.LogInformation($"✅ Login successful: {loginDto.Email}");
                return Ok(new { data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"⚠️ Login blocked - email not verified: {loginDto.Email}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error during login for {loginDto.Email}");
                return StatusCode(500, new { message = "Erreur serveur lors de la connexion" });
            }
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            try
            {
                _logger.LogInformation($"✉️ Email verification attempt with token: {dto.Token?.Substring(0, 10)}...");

                if (string.IsNullOrWhiteSpace(dto.Token))
                {
                    _logger.LogWarning("❌ Verification failed - missing token");
                    return BadRequest(new { message = "Token de vérification manquant" });
                }

                var result = await _authService.VerifyEmailAsync(dto.Token);

                if (!result)
                {
                    _logger.LogWarning($"❌ Verification failed - invalid or expired token");
                    return BadRequest(new { message = "Le token est invalide ou a expiré" });
                }

                _logger.LogInformation($"✅ Email verified successfully");
                return Ok(new { message = "Email vérifié avec succès!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during email verification");
                return StatusCode(500, new { message = "Erreur serveur lors de la vérification" });
            }
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
        {
            try
            {
                _logger.LogInformation($"📧 ===== RESEND VERIFICATION REQUEST =====");
                _logger.LogInformation($"📧 Email: {dto.Email}");

                // ✅ Validation
                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    _logger.LogWarning("❌ Resend failed - missing email");
                    return BadRequest(new { message = "Adresse email requise" });
                }

                if (!IsValidEmail(dto.Email))
                {
                    _logger.LogWarning($"❌ Resend failed - invalid email format: {dto.Email}");
                    return BadRequest(new { message = "Format d'email invalide" });
                }

                _logger.LogInformation($"✅ Email validation passed");
                _logger.LogInformation($"🔍 Calling ResendVerificationEmailAsync...");

                var result = await _authService.ResendVerificationEmailAsync(dto.Email);

                _logger.LogInformation($"📧 ResendVerificationEmailAsync returned: {result}");

                if (!result)
                {
                    _logger.LogWarning($"❌ Resend failed - service returned false for {dto.Email}");
                    return BadRequest(new
                    {
                        message = "Impossible de renvoyer l'email. Vérifiez que l'adresse existe et n'est pas déjà vérifiée."
                    });
                }

                _logger.LogInformation($"✅ Verification email resent successfully to {dto.Email}");
                return Ok(new
                {
                    message = "Email de vérification renvoyé avec succès! Vérifiez votre boîte de réception."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error resending verification email to {dto.Email}");
                _logger.LogError($"   Message: {ex.Message}");
                _logger.LogError($"   Stack: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Erreur serveur lors de l'envoi de l'email"
                });
            }
        }

        // ✅ NEW ENDPOINT: Debug user status
        [HttpGet("debug-user/{email}")]
        [AllowAnonymous] // Remove this in production!
        public async Task<IActionResult> DebugUser(string email)
        {
            try
            {
                var user = await _authService.GetUserByEmailAsync(email);

                if (user == null)
                {
                    return NotFound(new { message = "User not found", email });
                }

                return Ok(new
                {
                    email = user.Email,
                    emailConfirmed = user.EmailConfirmed,
                    isActive = user.IsActive,
                    hasVerificationToken = !string.IsNullOrEmpty(user.EmailVerificationToken),
                    tokenExpires = user.EmailVerificationTokenExpires,
                    createdAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error debugging user {email}");
                return StatusCode(500, new { message = ex.Message });
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
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "Erreur serveur" });
            }
        }

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

    // DTOs
    public class VerifyEmailDto
    {
        public string Token { get; set; } = string.Empty;
    }

    public class ResendVerificationDto
    {
        public string Email { get; set; } = string.Empty;
    }
}