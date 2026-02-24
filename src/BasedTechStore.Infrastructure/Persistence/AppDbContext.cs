using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BasedTechStore.Application.Common.Interfaces.Persistence;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Entities.Orders;
using BasedTechStore.Domain.Entities.Specifications;
using BasedTechStore.Domain.Entities.Cart;
using Microsoft.AspNetCore.Identity;

namespace BasedTechStore.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole, string>, IAppDbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<SpecificationType> SpecificationTypes { get; set; }
        public DbSet<SpecificationCategory> SpecificationCategories { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Category -> SubCategory(one-to-many)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(sc => sc.Category)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // SubCategory -> Product(one-to-many)
            modelBuilder.Entity<SubCategory>()
                .HasMany(c => c.Products)
                .WithOne(p => p.SubCategory)
                .HasForeignKey(sc => sc.SubCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart -> CartItem(one-to-many)
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart -> AppUser(many-to-one, optional)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired(false) // Optional relationship
                .OnDelete(DeleteBehavior.SetNull);

            // CartItem -> Product(many-to-one)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> OrderItem(one-to-many)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Product(many-to-one)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            // Order -> AppUser(many-to-one)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // SpecificationCategory -> SpecificationType(many-to-many)
            modelBuilder.Entity<SpecificationCategory>()
                .HasOne(sc => sc.ProductCategory)
                .WithMany()
                .HasForeignKey(sc => sc.ProductCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // SpecificationType -> SpecificationCategory(many-to-one)
            modelBuilder.Entity<SpecificationType>()
                .HasOne(st => st.SpecificationCategory)
                .WithMany(sc => sc.SpecificationTypes)
                .HasForeignKey(st => st.SpecificationCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductSpecification -> Product(many-to-one)
            modelBuilder.Entity<ProductSpecification>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSpecifications)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductSpecification -> SpecificationType(many-to-one)
            modelBuilder.Entity<ProductSpecification>()
                .HasOne(ps => ps.SpecificationType)
                .WithMany(st => st.ProductSpecifications)
                .HasForeignKey(ps => ps.SpecificationTypeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // =============== Indentity configuration =======================
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Role)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue(Domain.Constants.Roles.Customer);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.CustomPermissions)
                .HasMaxLength(2000);

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Role);

            // =============== Indexing for faster lookups ===================
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId);
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.SessionId);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Price)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.ProductId);

            modelBuilder.Entity<SpecificationCategory>()
                .HasIndex(sc => sc.ProductCategoryId);

            modelBuilder.Entity<SpecificationType>()
                .HasIndex(st => st.SpecificationCategoryId);

            modelBuilder.Entity<ProductSpecification>()
                .HasIndex(ps => ps.ProductId);
            modelBuilder.Entity<ProductSpecification>()
                .HasIndex(ps => ps.SpecificationTypeId);
            modelBuilder.Entity<ProductSpecification>()
                .Property(ps => ps.Value)
                .HasColumnType("nvarchar(max)");

            // RefreshToken indexes
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.JwtId);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.IsUsed });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            return Database.CanConnectAsync(cancellationToken);
        }
        public Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            return Database.MigrateAsync(cancellationToken);
        }
        public Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
        {
            return Database.GetPendingMigrationsAsync(cancellationToken);
        }
        public Task<IEnumerable<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
        {
            return Database.GetAppliedMigrationsAsync(cancellationToken);
        }
    }
}
