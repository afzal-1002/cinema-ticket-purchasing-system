using CinemaTicket.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Configure CASCADE DELETE: Cinema -> Screenings -> Reservations
            modelBuilder.Entity<Screening>()
                .HasOne(s => s.Cinema)
                .WithMany(c => c.Screenings)
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.Cascade); // Delete screenings when cinema deleted

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Screening)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade); // Delete reservations when screening deleted

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete reservations when user deleted

            // ✅ Unique constraint: One seat per screening
            modelBuilder.Entity<Reservation>()
                .HasIndex(r => new { r.ScreeningId, r.SeatNumber })
                .IsUnique();

            // ✅ Index for username lookup
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // ✅ CONCURRENCY: Configure RowVersion as concurrency token for User
            modelBuilder.Entity<User>()
                .Property(u => u.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // ✅ CONCURRENCY: Configure RowVersion as concurrency token for Reservation
            modelBuilder.Entity<Reservation>()
                .Property(r => r.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // ✅ TIMESTAMP: Configure CreatedAt with default value
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // ✅ ADDED: Explicitly configure Reservation -> User relationship
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ ADDED: Explicitly configure Reservation -> Screening relationship
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Screening)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add other entity configurations as needed
        }
    }
}
