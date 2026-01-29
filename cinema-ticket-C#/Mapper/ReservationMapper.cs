using CinemaTicket.Dto.Reservation;
using CinemaTicket.Model;

namespace CinemaTicket.Mapper;

public static class ReservationMapper
{
    public static ReservationSummaryDto ToSummary(this Reservation reservation)
    {
        return new ReservationSummaryDto(
            reservation.Id,
            reservation.ScreeningId,
            reservation.UserId,
            reservation.Row,
            reservation.Seat,
            reservation.Status,
            reservation.CreatedAt,
            reservation.RowVersion
        );
    }
}
