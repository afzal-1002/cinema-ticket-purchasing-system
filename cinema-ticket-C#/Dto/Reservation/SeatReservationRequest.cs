using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Dto.Reservation;

public class SeatReservationRequest
{
    [Required]
    public long ScreeningId { get; set; }

    [Range(1, int.MaxValue)]
    public int Row { get; set; }

    [Range(1, int.MaxValue)]
    public int Seat { get; set; }
}
