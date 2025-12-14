using System.ComponentModel.DataAnnotations;

namespace MielShop.API.DTOs.User
{
    // DTO pour la liste des utilisateurs
    public class UserListDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO pour les détails d'un utilisateur
    public class UserDetailDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    // DTO pour créer un utilisateur
    public class CreateUserDto
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Le rôle est requis")]
        public string Role { get; set; } = "Customer";
    }

    // DTO pour mettre à jour un utilisateur
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Le rôle est requis")]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    // DTO pour changer le rôle
    public class ChangeRoleDto
    {
        [Required(ErrorMessage = "Le rôle est requis")]
        public string Role { get; set; } = string.Empty;
    }

    // DTO pour activer/désactiver
    public class ToggleStatusDto
    {
        [Required(ErrorMessage = "Le statut est requis")]
        public bool IsActive { get; set; }
    }

    // DTO pour réinitialiser le mot de passe
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string NewPassword { get; set; } = string.Empty;
    }

    // DTO pour la réponse paginée
    public class PaginatedUsersDto
    {
        public List<UserListDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // DTO pour le résultat des opérations
    public class UserOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }

    // DTO pour les statistiques
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int AdminCount { get; set; }
        public int CustomerCount { get; set; }
        public int NewUsersThisMonth { get; set; }
    }
}