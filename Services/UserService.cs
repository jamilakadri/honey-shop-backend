using Microsoft.EntityFrameworkCore;
using MielShop.API.Data;
using MielShop.API.DTOs.User;
using MielShop.API.Models;
using BCrypt.Net; // ✅ Added BCrypt

namespace MielShop.API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedUsersDto> GetUsersAsync(int pageNumber, int pageSize, string? searchTerm, string? role)
        {
            var query = _context.Users.AsQueryable();

            // Filtrer par terme de recherche
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    u.Email.Contains(searchTerm) ||
                    u.FirstName.Contains(searchTerm) ||
                    u.LastName.Contains(searchTerm));
            }

            // Filtrer par rôle
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            // Compter le total
            var totalCount = await query.CountAsync();

            // Pagination
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return new PaginatedUsersDto
            {
                Data = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return null;

            return new UserDetailDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }

        public async Task<UserOperationResult> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Vérifier si l'email existe déjà
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == createUserDto.Email);
            if (existingUser != null)
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Cet email est déjà utilisé"
                };
            }

            // Valider le rôle
            if (createUserDto.Role != "Admin" && createUserDto.Role != "Customer")
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Rôle invalide. Utilisez 'Admin' ou 'Customer'"
                };
            }

            // Créer le nouvel utilisateur
            var user = new User
            {
                Email = createUserDto.Email,
                PasswordHash = HashPassword(createUserDto.Password), // ✅ Now uses BCrypt
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PhoneNumber = createUserDto.PhoneNumber,
                Role = createUserDto.Role,
                IsActive = true,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserOperationResult
            {
                Success = true,
                Message = "Utilisateur créé avec succès",
                UserId = user.UserId
            };
        }

        public async Task<UserOperationResult> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            // Valider le rôle
            if (updateUserDto.Role != "Admin" && updateUserDto.Role != "Customer")
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Rôle invalide. Utilisez 'Admin' ou 'Customer'"
                };
            }

            // Mettre à jour les informations
            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.Role = updateUserDto.Role;
            user.IsActive = updateUserDto.IsActive;

            await _context.SaveChangesAsync();

            return new UserOperationResult
            {
                Success = true,
                Message = "Utilisateur mis à jour avec succès"
            };
        }

        public async Task<UserOperationResult> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return new UserOperationResult
            {
                Success = true,
                Message = "Utilisateur supprimé avec succès"
            };
        }

        public async Task<UserOperationResult> ChangeUserRoleAsync(int userId, string newRole)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            // Valider le rôle
            if (newRole != "Admin" && newRole != "Customer")
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Rôle invalide. Utilisez 'Admin' ou 'Customer'"
                };
            }

            user.Role = newRole;
            await _context.SaveChangesAsync();

            return new UserOperationResult
            {
                Success = true,
                Message = "Rôle modifié avec succès"
            };
        }

        public async Task<UserOperationResult> ToggleUserStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();

            return new UserOperationResult
            {
                Success = true,
                Message = $"Utilisateur {(isActive ? "activé" : "désactivé")} avec succès"
            };
        }

        public async Task<UserOperationResult> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new UserOperationResult
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            user.PasswordHash = HashPassword(newPassword); // ✅ Now uses BCrypt
            await _context.SaveChangesAsync();

            return new UserOperationResult
            {
                Success = true,
                Message = "Mot de passe réinitialisé avec succès"
            };
        }

        public async Task<UserStatsDto> GetUserStatsAsync()
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var inactiveUsers = totalUsers - activeUsers;
            var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
            var customerCount = await _context.Users.CountAsync(u => u.Role == "Customer");
            var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= firstDayOfMonth);

            return new UserStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                AdminCount = adminCount,
                CustomerCount = customerCount,
                NewUsersThisMonth = newUsersThisMonth
            };
        }

        // ✅ UPDATED: Now uses BCrypt instead of SHA256
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}