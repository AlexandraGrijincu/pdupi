using Gym.Models; // Asigură-te că acest namespace corespunde locului unde ai clasele User, GymClass etc.
using Microsoft.EntityFrameworkCore;

namespace Gym.Data
{
    public class AppDbContext : DbContext
    {
        // 1. Constructorul: permite aplicației să trimită setările (precum Connection String) către bază
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 2. Tabelele tale: Fiecare DbSet va deveni un tabel în pgAdmin
        public DbSet<User> Users { get; set; }
        public DbSet<GymClass> GymClasses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<GymRoom> GymRooms { get; set; }
        public DbSet<SportType> SportTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        // 3. Opțional: Aici poți configura detalii fine (de exemplu, nume de coloane cu spații)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Dacă vrei ca în tabelă să apară "Nume complet" în loc de "FullName"
            // modelBuilder.Entity<User>().Property(u => u.FullName).HasColumnName("Nume complet");
        }
    }
}