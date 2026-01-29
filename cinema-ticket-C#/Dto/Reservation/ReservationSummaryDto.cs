using CinemaTicket.Model;

namespace CinemaTicket.Dto.Reservation;

public record ReservationSummaryDto(
    long Id,
    long ScreeningId,
    long UserId,
    int Row,
    int Seat,
    ReservationStatus Status,
    DateTime CreatedAt,
    byte[] RowVersion
);
