using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MielShop.API.Data;
using MielShop.API.Services;
using MielShop.API.Helpers;
using MielShop.API.Repositories;
using MielShop.API.Models;
using Microsoft.Extensions.FileProviders;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ✅ FIX POSTGRESQL DATETIME
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ============================================
// 🗄️ DATABASE CONFIGURATION
// ============================================
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

Console.WriteLine($"🔍 DATABASE_URL exists: {!string.IsNullOrEmpty(connectionString)}");

// Check for DATABASE_URL from environment (Railway automatically provides this)
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("🔍 Found DATABASE_URL environment variable");

    // Railway provides postgres:// format, convert to Npgsql format
    if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
    {
        try
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');

            // Changed 'builder' to 'connBuilder' to avoid naming conflict
            var connBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = userInfo[0],
                Password = userInfo.Length > 1 ? userInfo[1] : "",
                Database = uri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true,
                // Add these for Railway compatibility
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
            Console.WriteLine($"❌ Raw DATABASE_URL value: {connectionString}");
            throw;
        }
    }
    else
    {
        Console.WriteLine("✅ Using DATABASE_URL as-is (already in connection string format)");
    }
}
else
{
    Console.WriteLine("⚠️ DATABASE_URL not found, using appsettings.json");
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

/// ============================================
// 📧 EMAIL CONFIGURATION (Resend)
// ============================================
builder.Services.AddHttpClient();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

Console.WriteLine("📧 Email service configured with Resend");

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
    throw new InvalidOperationException("JWT Secret key is not configured in appsettings.json");
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
// 🌐 CORS CONFIGURATION - UPDATED
// ============================================
// ============================================
// 🌐 CORS CONFIGURATION - FIXED
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        // Get allowed origins from configuration or environment
        var allowedOrigins = new List<string>
        {
            "https://honey-shop-production-9137.up.railway.app",  // ✅ Your frontend URL
            "http://localhost:4200"  // ✅ For local development
        };

        // Add Railway frontend URL if available from environment variable
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
        // ❌ REMOVED: .SetIsOriginAllowedToAllowWildcardSubdomains()
        // This was causing CORS to fail!
    });
});

// Controllers with JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Swagger
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

// ============================================
// 📁 STATIC FILES CONFIGURATION
// ============================================
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Console.WriteLine($"✅ Created uploads directory: {uploadsPath}");
}

var productsPath = Path.Combine(uploadsPath, "products");
var categoriesPath = Path.Combine(uploadsPath, "categories");

if (!Directory.Exists(productsPath))
{
    Directory.CreateDirectory(productsPath);
    Console.WriteLine($"✅ Created products directory: {productsPath}");
}

if (!Directory.Exists(categoriesPath))
{
    Directory.CreateDirectory(categoriesPath);
    Console.WriteLine($"✅ Created categories directory: {categoriesPath}");
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

Console.WriteLine($"✅ Static files configured for: {uploadsPath}");
Console.WriteLine($"✅ Accessible at: /uploads/");

app.UseStaticFiles();

// ✅ IMPORTANT: CORS must come BEFORE Authentication and Authorization
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
    environment = app.Environment.EnvironmentName
}));

app.MapControllers();

// ============================================
// 🌐 ENVIRONMENT CONFIGURATION LOGGING
// ============================================
var backendUrl = Environment.GetEnvironmentVariable("RAILWAY_PUBLIC_DOMAIN");
if (!string.IsNullOrEmpty(backendUrl))
{
    backendUrl = $"https://{backendUrl}";
}
else
{
    backendUrl = builder.Configuration["AppSettings:BackendUrl"] ?? "http://localhost:5198";
}

Console.WriteLine("🚀 Application started");
Console.WriteLine($"🌐 Backend URL: {backendUrl}");
Console.WriteLine($"📁 Uploads path: {uploadsPath}");
Console.WriteLine($"🖼️ Products images: {productsPath}");
Console.WriteLine($"🗂️ Categories images: {categoriesPath}");

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
        Console.WriteLine($"❌ Error migrating database Djamila");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
}

app.Run();