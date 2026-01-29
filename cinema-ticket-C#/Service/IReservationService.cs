using CinemaTicket.Dto.Reservation;
using CinemaTicket.Dto.Seat;
using CinemaTicket.Model;

namespace CinemaTicket.Service;

public interface IReservationService
{
    Task<Reservation> ReserveSeatAsync(long userId, SeatReservationRequest request, CancellationToken cancellationToken);
    Task CancelReservationAsync(long userId, CancelReservationRequest request, CancellationToken cancellationToken);
    Task<SeatMapResponse> GetSeatMapAsync(long screeningId, CancellationToken cancellationToken);
}
