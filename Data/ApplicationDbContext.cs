using Microsoft.EntityFrameworkCore;
using MielShop.API.Models;

namespace MielShop.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<PromoCodeUsage> PromoCodeUsages { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Categories
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();
            });

            // Products
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ProductImages
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasOne(pi => pi.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductAttributes
            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.HasOne(pa => pa.Product)
                    .WithMany(p => p.ProductAttributes)
                    .HasForeignKey(pa => pa.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Reviews
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasIndex(e => new { e.ProductId, e.UserId }).IsUnique();
                entity.HasIndex(e => e.ProductId);

                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Carts
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.SessionId).IsUnique();

                entity.HasOne(c => c.User)
                    .WithOne(u => u.Cart)
                    .HasForeignKey<Cart>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItems
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(e => new { e.CartId, e.ProductId }).IsUnique();
                entity.HasIndex(e => e.CartId);

                entity.HasOne(ci => ci.Cart)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Product)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Addresses
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.HasOne(a => a.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.OrderStatus);
                entity.HasIndex(e => e.OrderDate);

                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // OrderItems
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasIndex(e => e.OrderId);

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Payments
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(e => e.TransactionId).IsUnique();

                entity.HasOne(p => p.Order)
                    .WithMany(o => o.Payments)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PromoCodes
            modelBuilder.Entity<PromoCode>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // PromoCodeUsage
            modelBuilder.Entity<PromoCodeUsage>(entity =>
            {
                entity.HasIndex(e => new { e.PromoCodeId, e.UserId, e.OrderId }).IsUnique();

                entity.HasOne(pcu => pcu.PromoCode)
                    .WithMany(pc => pc.PromoCodeUsages)
                    .HasForeignKey(pcu => pcu.PromoCodeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pcu => pcu.User)
                    .WithMany()
                    .HasForeignKey(pcu => pcu.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pcu => pcu.Order)
                    .WithMany()
                    .HasForeignKey(pcu => pcu.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Wishlists
            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

                entity.HasOne(w => w.User)
                    .WithMany(u => u.Wishlists)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Product)
                    .WithMany(p => p.Wishlists)
                    .HasForeignKey(w => w.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Notifications
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SiteSettings
            modelBuilder.Entity<SiteSetting>(entity =>
            {
                entity.HasIndex(e => e.SettingKey).IsUnique();
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            ConvertDatesToUtc(); // ✅ ADDED: Convert all DateTime to UTC
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            ConvertDatesToUtc(); // ✅ ADDED: Convert all DateTime to UTC
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is User user)
                {
                    if (entry.State == EntityState.Added)
                        user.CreatedAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Product product)
                {
                    if (entry.State == EntityState.Added)
                        product.CreatedAt = DateTime.UtcNow;
                    product.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Order order)
                {
                    if (entry.State == EntityState.Added)
                        order.CreatedAt = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Cart cart)
                {
                    if (entry.State == EntityState.Added)
                        cart.CreatedAt = DateTime.UtcNow;
                    cart.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is CartItem cartItem)
                {
                    if (entry.State == EntityState.Added)
                        cartItem.CreatedAt = DateTime.UtcNow;
                    cartItem.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        // ✅ NEW METHOD: Convert all DateTime properties to UTC for PostgreSQL compatibility
        private void ConvertDatesToUtc()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    // Handle DateTime properties
                    if (property.Metadata.ClrType == typeof(DateTime))
                    {
                        var dateTime = (DateTime)property.CurrentValue;

                        // Convert Unspecified to UTC
                        if (dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                        }
                        // Convert Local to UTC
                        else if (dateTime.Kind == DateTimeKind.Local)
                        {
                            property.CurrentValue = dateTime.ToUniversalTime();
                        }
                        // Already UTC - no change needed
                    }
                    // Handle nullable DateTime properties
                    else if (property.Metadata.ClrType == typeof(DateTime?))
                    {
                        var dateTime = (DateTime?)property.CurrentValue;

                        if (dateTime.HasValue)
                        {
                            // Convert Unspecified to UTC
                            if (dateTime.Value.Kind == DateTimeKind.Unspecified)
                            {
                                property.CurrentValue = DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
                            }
                            // Convert Local to UTC
                            else if (dateTime.Value.Kind == DateTimeKind.Local)
                            {
                                property.CurrentValue = dateTime.Value.ToUniversalTime();
                            }
                            // Already UTC - no change needed
                        }
                    }
                }
            }
        }
    }

}