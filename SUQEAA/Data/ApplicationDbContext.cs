using Microsoft.EntityFrameworkCore;
using WebApplication3.Models; 
using SQ.Models; 

namespace WebApplication3.Data  
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

         public DbSet<Customer> Customer { get; set; }
        public DbSet<District> District { get; set; }
        public DbSet<Driver> Driver { get; set; }
        public DbSet<Region> Region { get; set; }
        public DbSet<Request> Request { get; set; }
        public DbSet<Tank> Tank { get; set; }
        public DbSet<SimpleUser> SimpleUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

             modelBuilder.Entity<Customer>()
                .HasOne(c => c.Region)
                .WithMany(r => r.Customers)
                .HasForeignKey(c => c.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Customer>()
                .HasOne(c => c.District)
                .WithMany(d => d.Customers)
                .HasForeignKey(c => c.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Tank>()
                .HasOne(t => t.Region)
                .WithMany()
                .HasForeignKey(t => t.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Tank>()
                .HasOne(t => t.District)
                .WithMany()
                .HasForeignKey(t => t.DistrictId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Customer>()
                .HasOne(c => c.SimpleUser)
                .WithOne()
                .HasForeignKey<Customer>(c => c.SimpleUserId)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Tank>()
                .HasOne(t => t.Driver)      
                .WithOne(d => d.Tank)       
                .HasForeignKey<Tank>(t => t.DriverId)  
                .OnDelete(DeleteBehavior.Restrict);  

            // إعداد دقة السعر في Tank
            modelBuilder.Entity<Tank>()
                .Property(t => t.PricePerLiter)
                .HasPrecision(18, 2);

             modelBuilder.Entity<ErrorViewModel>().HasNoKey();
        }
    }
}