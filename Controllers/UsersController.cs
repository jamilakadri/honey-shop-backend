using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.DTOs.User;
using MielShop.API.Services;
using System.Security.Claims;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Seuls les admins peuvent accéder à ce controller
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Récupérer tous les utilisateurs avec pagination et filtres
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? role = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _userService.GetUsersAsync(pageNumber, pageSize, searchTerm, role);

            return Ok(result);
        }

        /// <summary>
        /// Récupérer un utilisateur par ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Créer un nouvel utilisateur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.CreateUserAsync(createUserDto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = result.UserId },
                new { message = "Utilisateur créé avec succès", userId = result.UserId }
            );
        }

        /// <summary>
        /// Mettre à jour un utilisateur
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateUserAsync(id, updateUserDto);

            if (!result.Success)
            {
                return result.Message == "Utilisateur non trouvé"
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Utilisateur mis à jour avec succès" });
        }

        /// <summary>
        /// Supprimer un utilisateur
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Vérifier que l'admin ne supprime pas son propre compte
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim != null && int.Parse(currentUserIdClaim.Value) == id)
            {
                return BadRequest(new { message = "Vous ne pouvez pas supprimer votre propre compte" });
            }

            var result = await _userService.DeleteUserAsync(id);

            if (!result.Success)
            {
                return result.Message == "Utilisateur non trouvé"
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Utilisateur supprimé avec succès" });
        }

        /// <summary>
        /// Changer le rôle d'un utilisateur
        /// </summary>
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeRoleDto changeRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que l'admin ne change pas son propre rôle
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim != null && int.Parse(currentUserIdClaim.Value) == id)
            {
                return BadRequest(new { message = "Vous ne pouvez pas changer votre propre rôle" });
            }

            var result = await _userService.ChangeUserRoleAsync(id, changeRoleDto.Role);

            if (!result.Success)
            {
                return result.Message == "Utilisateur non trouvé"
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Rôle modifié avec succès" });
        }

        /// <summary>
        /// Activer/Désactiver un utilisateur
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleUserStatus(int id, [FromBody] ToggleStatusDto toggleStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que l'admin ne désactive pas son propre compte
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim != null && int.Parse(currentUserIdClaim.Value) == id)
            {
                return BadRequest(new { message = "Vous ne pouvez pas désactiver votre propre compte" });
            }

            var result = await _userService.ToggleUserStatusAsync(id, toggleStatusDto.IsActive);

            if (!result.Success)
            {
                return result.Message == "Utilisateur non trouvé"
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(new { message = $"Utilisateur {(toggleStatusDto.IsActive ? "activé" : "désactivé")} avec succès" });
        }

        /// <summary>
        /// Réinitialiser le mot de passe d'un utilisateur
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ResetPasswordAsync(id, resetPasswordDto.NewPassword);

            if (!result.Success)
            {
                return result.Message == "Utilisateur non trouvé"
                    ? NotFound(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Mot de passe réinitialisé avec succès" });
        }

        /// <summary>
        /// Obtenir les statistiques des utilisateurs
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var stats = await _userService.GetUserStatsAsync();
            return Ok(stats);
        }
    }
}