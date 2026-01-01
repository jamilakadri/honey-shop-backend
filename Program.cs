using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MielShop.API.Data;
using MielShop.API.Services;
using MielShop.API.Helpers;
using MielShop.API.Repositories;
using MielShop.API.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ✅ FIX POSTGRESQL DATETIME
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ============================================
// 🗄️ DATABASE CONFIGURATION
// ============================================
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

Console.WriteLine($"🔍 DATABASE_URL exists: {!string.IsNullOrEmpty(connectionString)}");

if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("🔍 Found DATABASE_URL environment variable");

    if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
    {
        try
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');

            var connBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = userInfo[0],
                Password = userInfo.Length > 1 ? userInfo[1] : "",
                Database = uri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true,
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = 100,
                ConnectionIdleLifetime = 300,
                ConnectionPruningInterval = 10
            };

            connectionString = connBuilder.ToString();
            Console.WriteLine($"✅ Converted to Npgsql format: {uri.Host}:{connBuilder.Port}/{connBuilder.Database}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
            throw;
        }
    }
}
else
{
    Console.WriteLine("⚠️ DATABASE_URL not found, using appsettings.json");
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================
// 📧 EMAIL VALIDATION CONFIGURATION (AbstractAPI)
// ============================================
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailValidationService, EmailValidationService>();

Console.WriteLine("📧 Email validation service configured with AbstractAPI");

// ============================================
// 📷 CLOUDINARY CONFIGURATION
// ============================================
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

Console.WriteLine("📷 Cloudinary service configured");

// ============================================
// ⭐ SERVICE REGISTRATION
// ============================================

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();

// Business Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtService>();

// User Management Service
builder.Services.AddScoped<IUserService, UserService>();

// ============================================
// 🔐 JWT CONFIGURATION
// ============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret key is not configured");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ============================================
// 🌐 CORS CONFIGURATION
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        var allowedOrigins = new List<string>
        {
            "https://mieldeaoussou.up.railway.app",
            "http://localhost:4200"
        };

        var railwayFrontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        if (!string.IsNullOrEmpty(railwayFrontendUrl) && !allowedOrigins.Contains(railwayFrontendUrl))
        {
            allowedOrigins.Add(railwayFrontendUrl);
            Console.WriteLine($"✅ Added Railway frontend to CORS: {railwayFrontendUrl}");
        }

        Console.WriteLine($"🌐 CORS allowed origins: {string.Join(", ", allowedOrigins)}");

        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================================
// 🔧 MIDDLEWARE PIPELINE
// ============================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ============================================
// 🏥 HEALTH CHECK ENDPOINT
// ============================================
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    abstractApiConfigured = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ABSTRACT_API_KEY"))
}));

app.MapControllers();

Console.WriteLine("🚀 Application started");
Console.WriteLine($"📧 AbstractAPI configured: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ABSTRACT_API_KEY"))}");

// ============================================
// 🗄️ DATABASE MIGRATION
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("🔄 Applying database migrations...");
        context.Database.Migrate();
        Console.WriteLine("✅ Database migrated successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error migrating database: {ex.Message}");
        throw;
    }
}

app.Run();