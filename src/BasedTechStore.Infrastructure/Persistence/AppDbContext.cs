using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BasedTechStore.Application.Common.Interfaces.Persistence;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Entities.Orders;
using System.Net.Http.Headers;
using BasedTechStore.Domain.Entities.Specifications;

namespace BasedTechStore.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser, AppUserRole, string>, IAppDbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<SpecificationType> SpecificationTypes { get; set; }
        public DbSet<SpecificationCategory> SpecificationCategories { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<AppUser>().ToTable("Users");
            //modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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

            // Indexing for faster lookups
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
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
