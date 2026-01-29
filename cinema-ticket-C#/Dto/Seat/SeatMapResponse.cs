namespace CinemaTicket.Dto.Seat;

public record SeatMapResponse(
    long ScreeningId,
    int Rows,
    int SeatsPerRow,
    IReadOnlyCollection<SeatInfo> Seats
);
