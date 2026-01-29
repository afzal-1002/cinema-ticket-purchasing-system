using CinemaTicket.Dto.Screening;
using CinemaTicket.Model;

namespace CinemaTicket.Mapper;

public static class ScreeningMapper
{
    public static ScreeningSummaryDto ToSummary(this Screening screening)
    {
        return new ScreeningSummaryDto(
            screening.Id,
            screening.CinemaId,
            screening.Cinema?.Name ?? string.Empty,
            screening.MovieId,
            screening.Movie?.Title ?? string.Empty,
            screening.StartDateTime,
            screening.TicketPrice
        );
    }

    public static ScreeningDetailDto ToDetail(this Screening screening)
    {
        var totalSeats = (screening.Cinema?.Rows ?? 0) * (screening.Cinema?.SeatsPerRow ?? 0);
        return new ScreeningDetailDto(
            screening.Id,
            screening.CinemaId,
            screening.Cinema?.Name ?? string.Empty,
            screening.Cinema?.Rows ?? 0,
            screening.Cinema?.SeatsPerRow ?? 0,
            screening.MovieId,
            screening.Movie?.Title ?? string.Empty,
            screening.Movie?.Description ?? string.Empty,
            screening.Movie?.DurationMinutes ?? 0,
            screening.StartDateTime,
            screening.TicketPrice,
            totalSeats, // Available seats would need reservation count
            totalSeats
        );
    }
}
