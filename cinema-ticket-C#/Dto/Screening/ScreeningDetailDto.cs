namespace CinemaTicket.Dto.Screening;

public record ScreeningDetailDto(
    long Id,
    long CinemaId,
    string CinemaName,
    int CinemaRows,
    int CinemaSeatsPerRow,
    long MovieId,
    string MovieTitle,
    string MovieDescription,
    int MovieDurationMinutes,
    DateTime StartDateTime,
    decimal TicketPrice,
    int AvailableSeats,
    int TotalSeats
);
