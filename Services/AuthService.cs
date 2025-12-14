using Microsoft.EntityFrameworkCore;
using MielShop.API.Data;
using MielShop.API.DTOs.Auth;
using MielShop.API.Helpers;
using MielShop.API.Models;
using BCrypt.Net;
using System.Security.Cryptography;

namespace MielShop.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> UpdatePhoneAsync(int userId, string phoneNumber);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ResendVerificationEmailAsync(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;

        public AuthService(ApplicationDbContext context, JwtService jwtService, IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Vérifier si l'email existe déjà
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return null; // Email déjà utilisé
            }

            // Hasher le mot de passe
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Générer un token de vérification
            var verificationToken = GenerateVerificationToken();

            // Créer le nouvel utilisateur
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = "Customer",
                IsActive = true,
                EmailConfirmed = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Envoyer l'email de vérification
            try
            {
                await _emailService.SendEmailVerificationAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    verificationToken
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur envoi email: {ex.Message}");
                // On continue même si l'email échoue
            }

            // Générer le token JWT
            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Trouver l'utilisateur par email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return null; // Utilisateur non trouvé
            }

            // Vérifier le mot de passe
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Mot de passe incorrect
            }

            // Vérifier si le compte est actif
            if (!user.IsActive)
            {
                return null; // Compte désactivé
            }

            // ✅ BLOQUER SI EMAIL NON VÉRIFIÉ
            if (!user.EmailConfirmed)
            {
                throw new UnauthorizedAccessException("Veuillez vérifier votre email avant de vous connecter");
            }

            // Mettre à jour la dernière connexion
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Générer le token JWT
            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                return false; // Token invalide
            }

            // Vérifier si le token a expiré
            if (user.EmailVerificationTokenExpires < DateTime.UtcNow)
            {
                return false; // Token expiré
            }

            // Marquer l'email comme vérifié
            user.EmailConfirmed = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpires = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Envoyer un email de bienvenue
            try
            {
                await _emailService.SendWelcomeEmailAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur envoi email bienvenue: {ex.Message}");
            }

            return true;
        }

        public async Task<bool> ResendVerificationEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return false; // Utilisateur non trouvé
            }

            if (user.EmailConfirmed)
            {
                return false; // Email déjà vérifié
            }

            // Générer un nouveau token
            var verificationToken = GenerateVerificationToken();
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Renvoyer l'email
            try
            {
                await _emailService.SendEmailVerificationAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    verificationToken
                );
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur renvoi email: {ex.Message}");
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Vérifier l'ancien mot de passe
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            // Hasher le nouveau mot de passe
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePhoneAsync(int userId, string phoneNumber)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.PhoneNumber = phoneNumber;
            await _context.SaveChangesAsync();
            return true;
        }

        // Générer un token de vérification sécurisé
        private string GenerateVerificationToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}