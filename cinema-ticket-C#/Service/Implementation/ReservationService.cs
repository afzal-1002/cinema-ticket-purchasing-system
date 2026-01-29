using CinemaTicket.Config;
using CinemaTicket.Dto.Reservation;
using CinemaTicket.Dto.Seat;
using CinemaTicket.Exception;
using CinemaTicket.Model;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Service.Implementation;

public class ReservationService(ApplicationDbContext context) : IReservationService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Reservation> ReserveSeatAsync(long userId, SeatReservationRequest request, CancellationToken cancellationToken)
    {
        await CleanupExpiredHoldsAsync(cancellationToken);

        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == request.ScreeningId, cancellationToken)
            ?? throw new DomainException("Screening not found.");

        if (request.Row > screening.Cinema.Rows || request.Seat > screening.Cinema.SeatsPerRow)
        {
            throw new DomainException("Seat coordinates exceed auditorium capacity.");
        }

        var hasReservation = await _context.Reservations.AnyAsync(
            r => r.ScreeningId == request.ScreeningId && r.Row == request.Row && r.Seat == request.Seat,
            cancellationToken);
        if (hasReservation)
        {
            throw new DomainException("Seat already reserved.");
        }

        var hasHold = await _context.SeatHolds.AnyAsync(
            h => h.ScreeningId == request.ScreeningId && h.Row == request.Row && h.Seat == request.Seat,
            cancellationToken);
        if (hasHold)
        {
            throw new DomainException("Seat currently on hold by another user. Please retry.");
        }

        var reservation = new Reservation
        {
            UserId = userId,
            ScreeningId = request.ScreeningId,
            Row = request.Row,
            Seat = request.Seat,
            Status = ReservationStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task CancelReservationAsync(long userId, CancelReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
                          ?? throw new DomainException("Reservation not found.");

        if (reservation.UserId != userId)
        {
            throw new DomainException("You can only cancel your own reservations.");
        }

        _context.Entry(reservation).Property(r => r.RowVersion).OriginalValue = request.RowVersion;
        _context.Reservations.Remove(reservation);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException("Reservation was already updated. Please refresh and retry.");
        }
    }

    public async Task<SeatMapResponse> GetSeatMapAsync(long screeningId, CancellationToken cancellationToken)
    {
        await CleanupExpiredHoldsAsync(cancellationToken);

        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == screeningId, cancellationToken)
            ?? throw new DomainException("Screening not found.");

        var reservations = await _context.Reservations
            .Where(r => r.ScreeningId == screeningId)
            .Select(r => new { r.Row, r.Seat })
            .ToListAsync(cancellationToken);

        var holds = await _context.SeatHolds
            .Where(h => h.ScreeningId == screeningId)
            .Select(h => new { h.Row, h.Seat })
            .ToListAsync(cancellationToken);

        var seats = new List<SeatInfo>();
        for (var row = 1; row <= screening.Cinema.Rows; row++)
        {
            for (var seat = 1; seat <= screening.Cinema.SeatsPerRow; seat++)
            {
                var reserved = reservations.Any(r => r.Row == row && r.Seat == seat);
                var held = holds.Any(h => h.Row == row && h.Seat == seat);
                seats.Add(new SeatInfo(row, seat, reserved, held));
            }
        }

        return new SeatMapResponse(screeningId, screening.Cinema.Rows, screening.Cinema.SeatsPerRow, seats);
    }

    private async Task CleanupExpiredHoldsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expired = await _context.SeatHolds
            .Where(h => h.ExpiresAt <= now || !h.IsActive)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            return;
        }

        _context.SeatHolds.RemoveRange(expired);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
