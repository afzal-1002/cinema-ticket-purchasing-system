using CinemaTicket.Config;
using CinemaTicket.Model;
using CinemaTicket.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaTicket.Service.Implementation;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger<DataSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Seed cinemas first
        await SeedCinemasAsync(cancellationToken);

        // Seed movies
        await SeedMoviesAsync(cancellationToken);

        // Seed screenings
        await SeedScreeningsAsync(cancellationToken);

        // Seed primary admin account
        await SeedUserAsync(
            email: "admin@gmail.com",
            firstName: "Admin",
            lastName: "User",
            phoneNumber: "+1234567890",
            password: "admin123",
            isAdmin: true,
            cancellationToken);

        // Seed secondary admin account
        await SeedUserAsync(
            email: "admin1@gmail.com",
            firstName: "Admin",
            lastName: "User",
            phoneNumber: "+1234567890",
            password: "admin123",
            isAdmin: true,
            cancellationToken);

        // Seed demo customer account
        await SeedUserAsync(
            email: "user@gmail.com",
            firstName: "Movie",
            lastName: "Fan",
            phoneNumber: "+1987654321",
            password: "user123",
            isAdmin: false,
            cancellationToken);

        // Legacy admin account
        await SeedUserAsync(
            email: "admin@cinema.local",
            firstName: "System",
            lastName: "Administrator",
            phoneNumber: "+1-555-0100",
            password: "Admin123!",
            isAdmin: true,
            cancellationToken);

        // Legacy customer account
        await SeedUserAsync(
            email: "customer@cinema.local",
            firstName: "Alex",
            lastName: "Customer",
            phoneNumber: "+1-555-0150",
            password: "User123!",
            isAdmin: false,
            cancellationToken);
    }

    private async Task SeedUserAsync(
        string email,
        string firstName,
        string lastName,
        string phoneNumber,
        string password,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
        {
            return;
        }

        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            IsAdmin = isAdmin,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = _passwordHasher.Hash(password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        
        string roleType = isAdmin ? "admin" : "customer";
        _logger.LogInformation("✅ Created {Role} user - Email: {Email}, Password: {Password}", 
            roleType, email, password);
    }

    private async Task SeedCinemasAsync(CancellationToken cancellationToken)
    {
        if (await _context.Cinemas.AnyAsync(cancellationToken))
        {
            return;
        }

        var cinemas = new[]
        {
            new Cinema { Name = "Cinema Grand Plaza", Rows = 10, SeatsPerRow = 15 },
            new Cinema { Name = "Cinema Royal", Rows = 8, SeatsPerRow = 12 },
            new Cinema { Name = "Cinema Star", Rows = 12, SeatsPerRow = 20 },
            new Cinema { Name = "Cinema Downtown", Rows = 6, SeatsPerRow = 10 },
            new Cinema { Name = "Cinema Luxury", Rows = 5, SeatsPerRow = 8 }
        };

        _context.Cinemas.AddRange(cinemas);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("✅ Initialized {Count} cinemas in the database", cinemas.Length);
    }

    private async Task SeedMoviesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Movies.AnyAsync(cancellationToken))
        {
            return;
        }

        var movies = new[]
        {
            new Movie
            {
                Title = "Dune: Part Two",
                Description = "Paul Atreides unites with the Fremen to unleash his destiny across Arrakis.",
                DurationMinutes = 165,
                Genre = "Sci-Fi",
                Rating = "PG-13",
                Director = "Denis Villeneuve",
                Cast = "Timothée Chalamet, Zendaya, Rebecca Ferguson",
                PosterUrl = "https://example.com/posters/dune2.jpg",
                TrailerUrl = "https://youtube.com/watch?v=dune2",
                ReleaseDate = new DateOnly(2024, 3, 1),
                IsActive = true
            },
            new Movie
            {
                Title = "Oppenheimer",
                Description = "The story of J. Robert Oppenheimer and the creation of the atomic bomb.",
                DurationMinutes = 180,
                Genre = "Drama",
                Rating = "R",
                Director = "Christopher Nolan",
                Cast = "Cillian Murphy, Emily Blunt, Matt Damon",
                PosterUrl = "https://example.com/posters/oppenheimer.jpg",
                TrailerUrl = "https://youtube.com/watch?v=oppenheimer",
                ReleaseDate = new DateOnly(2023, 7, 21),
                IsActive = true
            },
            new Movie
            {
                Title = "The Super Mario Bros. Movie",
                Description = "A plumber named Mario travels through the Mushroom Kingdom to save his brother.",
                DurationMinutes = 92,
                Genre = "Animation",
                Rating = "PG",
                Director = "Aaron Horvath & Michael Jelenic",
                Cast = "Chris Pratt, Anya Taylor-Joy, Jack Black",
                PosterUrl = "https://example.com/posters/mario.jpg",
                TrailerUrl = "https://youtube.com/watch?v=mario",
                ReleaseDate = new DateOnly(2023, 4, 5),
                IsActive = true
            }
        };

        _context.Movies.AddRange(movies);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("✅ Seeded default movies (Dune, Oppenheimer, Mario)");
    }

    private async Task SeedScreeningsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Screenings.AnyAsync(cancellationToken))
        {
            return;
        }

        var movies = await _context.Movies.ToListAsync(cancellationToken);
        var cinemas = await _context.Cinemas.ToListAsync(cancellationToken);

        if (movies.Count == 0 || cinemas.Count == 0)
        {
            return;
        }

        var screenings = new List<Screening>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < Math.Min(3, cinemas.Count); i++)
        {
            var screening = new Screening
            {
                CinemaId = cinemas[i % cinemas.Count].Id,
                MovieId = movies[i % movies.Count].Id,
                StartDateTime = now.AddDays(i + 1).Date.AddHours(19),
                TicketPrice = 14.99m + i
            };
            screenings.Add(screening);
        }

        _context.Screenings.AddRange(screenings);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("✅ Seeded default screenings using available movies and cinemas");
    }
}