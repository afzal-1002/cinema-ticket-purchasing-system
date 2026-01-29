namespace CinemaTicket.Dto.Screening;

public record ScreeningSummaryDto(
    long Id,
    long CinemaId,
    string CinemaName,
    long MovieId,
    string MovieTitle,
    DateTime StartDateTime,
    decimal TicketPrice
);
