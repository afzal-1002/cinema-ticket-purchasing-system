using CinemaTicket.Config;
using CinemaTicket.Model;
using CinemaTicket.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOnly)]
public class AnalyticsController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboardOverview(CancellationToken cancellationToken)
    {
        var totalReservations = await _context.Reservations.CountAsync(cancellationToken);
        var totalRevenue = await _context.Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed)
            .SumAsync(r => r.TotalPrice, cancellationToken);
        var totalScreenings = await _context.Screenings.CountAsync(cancellationToken);
        var activeMovies = await _context.Movies.CountAsync(m => m.IsActive, cancellationToken);

        return Ok(new
        {
            totalReservations,
            totalRevenue,
            totalScreenings,
            activeMovies,
            generatedAt = DateTime.UtcNow
        });
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<object>> GetRevenueAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var reservations = await _context.Reservations
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .Where(r => r.Status == ReservationStatus.Confirmed)
            .ToListAsync(cancellationToken);

        var totalRevenue = reservations.Sum(r => r.TotalPrice);
        var averageTicketPrice = reservations.Any() ? reservations.Average(r => r.TotalPrice / r.NumberOfSeats) : 0;
        var totalTicketsSold = reservations.Sum(r => r.NumberOfSeats);

        return Ok(new
        {
            startDate,
            endDate,
            totalRevenue,
            averageTicketPrice,
            totalTicketsSold,
            totalReservations = reservations.Count
        });
    }

    [HttpGet("popular-movies")]
    public async Task<ActionResult<object>> GetPopularMovies(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var popularMovies = await _context.Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed)
            .Include(r => r.Screening)
            .ThenInclude(s => s.Movie)
            .GroupBy(r => r.Screening.Movie)
            .Select(g => new
            {
                movieId = g.Key.Id,
                movieTitle = g.Key.Title,
                totalReservations = g.Count(),
                totalTicketsSold = g.Sum(r => r.NumberOfSeats),
                totalRevenue = g.Sum(r => r.TotalPrice)
            })
            .OrderByDescending(m => m.totalTicketsSold)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return Ok(popularMovies);
    }

    [HttpGet("occupancy/screening/{screeningId}")]
    public async Task<ActionResult<object>> GetScreeningOccupancy(long screeningId, CancellationToken cancellationToken)
    {
        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == screeningId, cancellationToken);

        if (screening == null)
        {
            return NotFound(new { message = $"Screening with ID {screeningId} not found" });
        }

        var totalSeats = screening.Cinema.Rows * screening.Cinema.SeatsPerRow;
        var reservedSeats = await _context.Reservations
            .Where(r => r.ScreeningId == screeningId && r.Status == ReservationStatus.Confirmed)
            .SumAsync(r => r.NumberOfSeats, cancellationToken);

        var heldSeats = await _context.SeatHolds
            .Where(h => h.ScreeningId == screeningId && h.ExpiresAt > DateTime.UtcNow)
            .CountAsync(cancellationToken);

        var availableSeats = totalSeats - reservedSeats - heldSeats;
        var occupancyRate = totalSeats > 0 ? (double)reservedSeats / totalSeats * 100 : 0;

        return Ok(new
        {
            screeningId,
            totalSeats,
            reservedSeats,
            heldSeats,
            availableSeats,
            occupancyRate = Math.Round(occupancyRate, 2)
        });
    }

    [HttpGet("occupancy/movies")]
    public async Task<ActionResult<object>> GetMovieOccupancy(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var movieOccupancy = await _context.Screenings
            .Include(s => s.Cinema)
            .Include(s => s.Movie)
            .GroupBy(s => s.Movie)
            .Select(g => new
            {
                movieId = g.Key.Id,
                movieTitle = g.Key.Title,
                totalScreenings = g.Count(),
                totalCapacity = g.Sum(s => s.Cinema.Rows * s.Cinema.SeatsPerRow),
                averageOccupancy = g.Average(s =>
                    _context.Reservations
                        .Where(r => r.ScreeningId == s.Id && r.Status == ReservationStatus.Confirmed)
                        .Sum(r => r.NumberOfSeats) * 100.0 / (s.Cinema.Rows * s.Cinema.SeatsPerRow))
            })
            .OrderByDescending(m => m.averageOccupancy)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return Ok(movieOccupancy);
    }

    [HttpGet("booking-trends")]
    public async Task<ActionResult<object>> GetBookingTrends(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var trends = await _context.Reservations
            .Where(r => r.CreatedAt >= startDate)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new
            {
                date = g.Key,
                totalBookings = g.Count(),
                totalRevenue = g.Sum(r => r.TotalPrice),
                confirmedBookings = g.Count(r => r.Status == ReservationStatus.Confirmed),
                cancelledBookings = g.Count(r => r.Status == ReservationStatus.Cancelled)
            })
            .OrderBy(t => t.date)
            .ToListAsync(cancellationToken);

        return Ok(new { days, trends });
    }
}
