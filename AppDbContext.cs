using Gym.Models; // Asigură-te că acest namespace corespunde locului unde ai clasele User, GymClass etc.
using Microsoft.EntityFrameworkCore;
using Gym.Services;

namespace Gym.Data
{
    public class AppDbContext : DbContext
    {
        private readonly int? _companyId;
        // 1. Constructorul: permite aplicației să trimită setările (precum Connection String) către bază
        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _companyId = tenantService.GetCompanyId();
        }

        // 2. Tabelele tale: Fiecare DbSet va deveni un tabel în pgAdmin
        public DbSet<User> Users { get; set; }
        public DbSet<GymClass> GymClasses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<GymRoom> GymRooms { get; set; }
        public DbSet<SportType> SportTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Company> Companies { get; set; }

        // 3. Opțional: Aici poți configura detalii fine (de exemplu, nume de coloane cu spații)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<GymClass>().HasQueryFilter(c => c.CompanyId == _companyId);
            modelBuilder.Entity<Booking>().HasQueryFilter(b => b.CompanyId == _companyId);
            modelBuilder.Entity<User>().HasQueryFilter(u => u.CompanyId == _companyId);
            //modelBuilder.Entity<GymRoom>().HasQueryFilter(r => r.CompanyId == _companyId);
            //modelBuilder.Entity<SportType>().HasQueryFilter(s => s.CompanyId == _companyId);
            modelBuilder.Entity<Subscription>().HasQueryFilter(s => s.CompanyId == _companyId);
        }
    }
}