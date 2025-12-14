using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.DTOs.Auth;
using MielShop.API.Services;
using System.Security.Claims;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (result == null)
            {
                return BadRequest(new { message = "Cet email est déjà utilisé" });
            }

            return Ok(new
            {
                message = "Inscription réussie. Veuillez vérifier votre email pour activer votre compte.",
                data = result
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginAsync(loginDto);

                if (result == null)
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect" });
                }

                return Ok(new
                {
                    message = "Connexion réussie",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Vérifier l'email avec le token
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.VerifyEmailAsync(verifyEmailDto.Token);

            if (!result)
            {
                return BadRequest(new { message = "Token invalide ou expiré" });
            }

            return Ok(new { message = "Email vérifié avec succès! Vous pouvez maintenant vous connecter." });
        }

        /// <summary>
        /// Renvoyer l'email de vérification
        /// </summary>
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationEmailDto resendDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResendVerificationEmailAsync(resendDto.Email);

            if (!result)
            {
                return BadRequest(new { message = "Impossible de renvoyer l'email. Vérifiez que l'email existe et n'est pas déjà vérifié." });
            }

            return Ok(new { message = "Email de vérification renvoyé avec succès. Vérifiez votre boîte de réception." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            var userId = int.Parse(userIdClaim.Value);
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phoneNumber = user.PhoneNumber,
                role = user.Role,
                isActive = user.IsActive,
                emailConfirmed = user.EmailConfirmed
            });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            var userId = int.Parse(userIdClaim.Value);
            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

            if (!result)
            {
                return BadRequest(new { message = "Mot de passe actuel incorrect" });
            }

            return Ok(new { message = "Mot de passe modifié avec succès" });
        }

        [HttpPost("update-phone")]
        [Authorize]
        public async Task<IActionResult> UpdatePhone([FromBody] UpdatePhoneDto updatePhoneDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            var userId = int.Parse(userIdClaim.Value);
            var result = await _authService.UpdatePhoneAsync(userId, updatePhoneDto.PhoneNumber);

            if (!result)
            {
                return BadRequest(new { message = "Échec de la mise à jour du numéro de téléphone" });
            }

            return Ok(new { message = "Numéro de téléphone mis à jour avec succès" });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { message = "Déconnexion réussie" });
        }
    }
}