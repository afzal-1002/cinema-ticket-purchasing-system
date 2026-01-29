using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Dto.Reservation;

public class CancelReservationRequest
{
    [Required]
    public long ReservationId { get; set; }

    [Required]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
