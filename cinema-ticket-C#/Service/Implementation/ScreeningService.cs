using CinemaTicket.Config;
using CinemaTicket.Dto.Screening;
using CinemaTicket.Exception;
using CinemaTicket.Model;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Service.Implementation;

public class ScreeningService(ApplicationDbContext context) : IScreeningService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Screening> CreateAsync(CreateScreeningRequest request, CancellationToken cancellationToken)
    {
        var cinema = await _context.Cinemas.FirstOrDefaultAsync(c => c.Id == request.CinemaId, cancellationToken)
                     ?? throw new DomainException("Cinema not found.");
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken)
                    ?? throw new DomainException("Movie not found.");

        var screening = new Screening
        {
            CinemaId = cinema.Id,
            MovieId = movie.Id,
            StartDateTime = request.StartDateTime,
            TicketPrice = request.TicketPrice,
            Cinema = cinema,
            Movie = movie
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync(cancellationToken);
        return screening;
    }

    public async Task DeleteAsync(long screeningId, CancellationToken cancellationToken)
    {
        var screening = await _context.Screenings
            .Include(s => s.Reservations)
            .Include(s => s.SeatHolds)
            .FirstOrDefaultAsync(s => s.Id == screeningId, cancellationToken)
            ?? throw new DomainException("Screening not found.");

        _context.Reservations.RemoveRange(screening.Reservations);
        _context.SeatHolds.RemoveRange(screening.SeatHolds);
        _context.Screenings.Remove(screening);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Screening>> ListUpcomingAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var screenings = await _context.Screenings
            .Include(s => s.Cinema)
            .Include(s => s.Movie)
            .Where(s => s.StartDateTime >= now)
            .OrderBy(s => s.StartDateTime)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return screenings;
    }

    public async Task<Screening> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new DomainException($"Screening with ID {id} not found");

        return screening;
    }
}
