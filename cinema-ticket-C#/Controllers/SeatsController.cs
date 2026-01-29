using CinemaTicket.Config;
using CinemaTicket.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CinemaTicket.Controllers;

[ApiController]
[Route("api/seats")]
public class SeatsController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    [HttpGet("screening/{screeningId}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetAvailableSeats(long screeningId, CancellationToken cancellationToken)
    {
        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == screeningId, cancellationToken);

        if (screening == null)
        {
            return NotFound(new { message = "Screening not found" });
        }

        var totalSeats = screening.Cinema.Rows * screening.Cinema.SeatsPerRow;

        var reservedSeats = await _context.Reservations
            .Where(r => r.ScreeningId == screeningId && r.Status == ReservationStatus.Confirmed)
            .SelectMany(r => Enumerable.Range(0, r.NumberOfSeats).Select((_, i) => new { r.Row, Seat = r.Seat + i }))
            .ToListAsync(cancellationToken);

        var heldSeats = await _context.SeatHolds
            .Where(h => h.ScreeningId == screeningId && h.ExpiresAt > DateTime.UtcNow)
            .Select(h => new { h.Row, h.Seat })
            .ToListAsync(cancellationToken);

        var occupiedSeats = reservedSeats.Concat(heldSeats).Distinct().ToList();

        return Ok(new
        {
            screeningId,
            rows = screening.Cinema.Rows,
            seatsPerRow = screening.Cinema.SeatsPerRow,
            totalSeats,
            availableSeats = totalSeats - occupiedSeats.Count,
            occupiedSeats
        });
    }

    [HttpPost("hold")]
    [Authorize]
    public async Task<ActionResult<object>> HoldSeats(
        [FromBody] HoldSeatsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == request.ScreeningId, cancellationToken);

        if (screening == null)
        {
            return NotFound(new { message = "Screening not found" });
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(10); // Hold seats for 10 minutes
        var holds = new List<SeatHold>();

        foreach (var seat in request.Seats)
        {
            // Check if seat is already occupied
            var isOccupied = await _context.Reservations
                .AnyAsync(r => r.ScreeningId == request.ScreeningId && 
                              r.Row == seat.Row && 
                              r.Seat == seat.Seat &&
                              r.Status == ReservationStatus.Confirmed, cancellationToken);

            if (isOccupied)
            {
                return BadRequest(new { message = $"Seat {seat.Row}-{seat.Seat} is already reserved" });
            }

            var hold = new SeatHold
            {
                ScreeningId = request.ScreeningId,
                UserId = userId,
                Row = seat.Row,
                Seat = seat.Seat,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };
            holds.Add(hold);
        }

        _context.SeatHolds.AddRange(holds);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Seats held successfully",
            holdIds = holds.Select(h => h.Id).ToList(),
            expiresAt,
            seats = holds.Select(h => new { h.Row, h.Seat }).ToList()
        });
    }

    [HttpDelete("hold")]
    [Authorize]
    public async Task<IActionResult> ReleaseHolds(
        [FromBody] ReleaseHoldsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var holds = await _context.SeatHolds
            .Where(h => request.HoldIds.Contains(h.Id) && h.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.SeatHolds.RemoveRange(holds);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("hold/all")]
    [Authorize]
    public async Task<IActionResult> ReleaseAllHolds(CancellationToken cancellationToken)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var holds = await _context.SeatHolds
            .Where(h => h.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.SeatHolds.RemoveRange(holds);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("hold/my-holds")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyHolds(CancellationToken cancellationToken)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var holds = await _context.SeatHolds
            .Where(h => h.UserId == userId && h.ExpiresAt > DateTime.UtcNow)
            .Include(h => h.Screening)
            .ThenInclude(s => s.Movie)
            .Include(h => h.Screening)
            .ThenInclude(s => s.Cinema)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new
            {
                holdId = h.Id,
                screeningId = h.ScreeningId,
                row = h.Row,
                seat = h.Seat,
                expiresAt = h.ExpiresAt,
                screening = new
                {
                    id = h.Screening.Id,
                    movieTitle = h.Screening.Movie.Title,
                    cinemaName = h.Screening.Cinema.Name,
                    startDateTime = h.Screening.StartDateTime
                }
            })
            .ToListAsync(cancellationToken);

        return Ok(holds);
    }
}

public record HoldSeatsRequest(long ScreeningId, List<SeatPosition> Seats);
public record SeatPosition(int Row, int Seat);
public record ReleaseHoldsRequest(List<long> HoldIds);
