using CinemaTicket.Model;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Config;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Screening> Screenings => Set<Screening>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<SeatHold> SeatHolds => Set<SeatHold>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Cinema>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => new { r.ScreeningId, r.Row, r.Seat })
            .IsUnique();

        modelBuilder.Entity<SeatHold>()
            .HasIndex(h => new { h.ScreeningId, h.Row, h.Seat })
            .IsUnique();

        modelBuilder.Entity<Cinema>().HasData(
            new Cinema { Id = 1, Name = "Praga Multiplex", Rows = 12, SeatsPerRow = 20 },
            new Cinema { Id = 2, Name = "Ochota Classic", Rows = 8, SeatsPerRow = 15 },
            new Cinema { Id = 3, Name = "Mokotow Studio", Rows = 10, SeatsPerRow = 18 }
        );
    }
}
