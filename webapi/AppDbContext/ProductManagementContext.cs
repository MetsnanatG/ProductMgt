namespace webapi.AppDbContext
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using webapi.Models;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public class ProductManagementContext : IdentityDbContext<User>
    {
        public ProductManagementContext(DbContextOptions<ProductManagementContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductRequest> ProductRequests { get; set; }
        public DbSet<ApprovalHistory> ApprovalHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define relationships
            //modelBuilder.Entity<ProductRequest>()
            //.HasOne(r => r.Product)
            //.WithMany()
            //.HasForeignKey(r => r.ProductId);
            // Configure the relationship between ProductRequest and ApprovalHistory

            //     modelBuilder.Entity<Product>(entity =>
            //{
            //    entity.Property(e => e.Stock).IsRequired();

            //});

            // Configure relationships if necessary
            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany() // Assuming a user can create many products
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<ProductRequest>()
            .HasMany(a => a.ApprovalHistory)
            .WithOne(a => a.ProductRequest)
            .HasForeignKey(a => a.ProductRequestId);


            modelBuilder.Entity<ApprovalHistory>()
                        .HasOne(a => a.ProductRequest)
                        .WithMany(r => r.ApprovalHistory)
                        .HasForeignKey(a => a.ProductRequestId);
        }
    }
}
