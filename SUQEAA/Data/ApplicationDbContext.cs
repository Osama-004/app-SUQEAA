using Microsoft.EntityFrameworkCore;
using WebApplication3.Models; // تأكد من استيراد مساحة الاسم هذه لكياناتك
using SQ.Models; // هذا الاستيراد لكلاس ErrorViewModel كما هو موجود في الكود الأصلي الذي قدمته

namespace WebApplication3.Data // تأكد من أن مساحة الاسم هذه مطابقة لمجلد Data الخاص بك
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets = الجداول الفعلية في قاعدة البيانات
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

            // علاقة Region - Customer
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Region)
                .WithMany(r => r.Customers)
                .HasForeignKey(c => c.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة District - Customer
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.District)
                .WithMany(d => d.Customers)
                .HasForeignKey(c => c.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Tank - Region
            modelBuilder.Entity<Tank>()
                .HasOne(t => t.Region)
                .WithMany()
                .HasForeignKey(t => t.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Tank - District (قابلة للقيم الفارغة)
            modelBuilder.Entity<Tank>()
                .HasOne(t => t.District)
                .WithMany()
                .HasForeignKey(t => t.DistrictId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Customer - SimpleUser
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.SimpleUser)
                .WithOne()
                .HasForeignKey<Customer>(c => c.SimpleUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // <<<<<<<<<<< تكوين العلاقة واحد-إلى-واحد بين Tank و Driver
            modelBuilder.Entity<Tank>()
                .HasOne(t => t.Driver)      // الخزان له سائق واحد
                .WithOne(d => d.Tank)       // والسائق له خزان واحد
                .HasForeignKey<Tank>(t => t.DriverId) // DriverId في Tank هو المفتاح الخارجي
                .OnDelete(DeleteBehavior.Restrict); // يمنع حذف السائق إذا كان مرتبطًا بخزان

            // إعداد دقة السعر في Tank
            modelBuilder.Entity<Tank>()
                .Property(t => t.PricePerLiter)
                .HasPrecision(18, 2);

            // استثناء ViewModel غير المرتبط بقاعدة البيانات
            modelBuilder.Entity<ErrorViewModel>().HasNoKey();
        }
    }
}