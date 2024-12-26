using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<SuperBox> SuperBoxes { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<SuperBox>()
                .HasKey(sb => sb.Id);
            modelBuilder.Entity<SuperBox>()
                .Property(sb => sb.StreetName)
                .IsRequired();

            modelBuilder.Entity<SuperBox>()
                .Property(sb => sb.StreetNumber)
                .IsRequired();
            
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)  // Assuming a User has many Orders
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.SuperBox)
                .WithMany()  // Assuming a SuperBox can be referenced by many Orders
                .HasForeignKey(o => o.SuperBoxId);
        }
    }
}