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
                    message = "Inscription réussie!",
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error during login for {loginDto.Email}");
                return StatusCode(500, new { message = "Erreur serveur lors de la connexion" });
            }
        }

        [HttpGet("debug-user/{email}")]
        [AllowAnonymous]
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
}