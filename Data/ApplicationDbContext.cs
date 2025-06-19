using Microsoft.EntityFrameworkCore;
using MenuShop.Models;

namespace MenuShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision for Price
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 6, 18, 12, 0, 0);

            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin123", // In production, this should be hashed
                    FullName = "Administrator",
                    Email = "admin@menushop.com",
                    IsAdmin = true,
                    IsActive = true,
                    CreatedAt = now,
                    LastLoginAt = now
                }
            );

            // Seed categories
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Món chính",
                    Description = "Các món ăn chính",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Category
                {
                    Id = 2,
                    Name = "Món khai vị",
                    Description = "Các món khai vị",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Category
                {
                    Id = 3,
                    Name = "Món tráng miệng",
                    Description = "Các món tráng miệng",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );

            // Seed products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Cơm tấm sườn nướng",
                    Description = "Cơm tấm với sườn nướng thơm ngon",
                    Price = 45000,
                    Stock = 50,
                    CategoryId = 1,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Id = 2,
                    Name = "Phở bò",
                    Description = "Phở bò truyền thống",
                    Price = 55000,
                    Stock = 30,
                    CategoryId = 1,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Id = 3,
                    Name = "Gỏi cuốn",
                    Description = "Gỏi cuốn tôm thịt",
                    Price = 25000,
                    Stock = 40,
                    CategoryId = 2,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
        }
    }
} 