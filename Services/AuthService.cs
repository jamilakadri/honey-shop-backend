using Microsoft.EntityFrameworkCore;
using MielShop.API.Data;
using MielShop.API.DTOs.Auth;
using MielShop.API.Helpers;
using MielShop.API.Models;
using BCrypt.Net;

namespace MielShop.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> UpdatePhoneAsync(int userId, string phoneNumber);
        Task<User?> GetUserByEmailAsync(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IEmailValidationService _emailValidationService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            JwtService jwtService,
            IEmailValidationService emailValidationService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _emailValidationService = emailValidationService;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists in database
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == registerDto.Email.ToLower()))
            {
                _logger.LogWarning($"❌ Email already exists: {registerDto.Email}");
                throw new InvalidOperationException("EMAIL_ALREADY_EXISTS");
            }

            // ✅ VALIDATE EMAIL WITH ABSTRACTAPI - BLOCK IF INVALID
            bool emailIsReal = await _emailValidationService.IsEmailRealAsync(registerDto.Email);

            _logger.LogInformation($"📧 Email validation for {registerDto.Email}: {(emailIsReal ? "REAL ✅" : "FAKE ❌")}");

            if (!emailIsReal)
            {
                _logger.LogWarning($"❌ Registration BLOCKED - Invalid email: {registerDto.Email}");
                throw new InvalidOperationException("INVALID_EMAIL");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create new user - only if email is valid
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = "Customer",
                IsActive = true,
                EmailConfirmed = true, // ✅ Auto-confirm since AbstractAPI validated it
                EmailVerificationToken = null,
                EmailVerificationTokenExpires = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ User registered successfully: {user.Email} (EmailConfirmed: {user.EmailConfirmed})");

            // Generate JWT token
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

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
            {
                _logger.LogWarning($"❌ User not found: {loginDto.Email}");
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning($"❌ Invalid password for: {loginDto.Email}");
                return null;
            }

            // Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning($"❌ Account inactive: {loginDto.Email}");
                return null;
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Login successful: {loginDto.Email} (EmailConfirmed: {user.EmailConfirmed})");

            // Generate JWT token
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

            // Verify old password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            // Hash new password
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
    }
}