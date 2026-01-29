using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Model;

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled
}

[Table("reservations")]
public class Reservation
{
    public long Id { get; set; }

    [Required]
    public User User { get; set; } = null!;
    public long UserId { get; set; }

    [Required]
    public Screening Screening { get; set; } = null!;
    public long ScreeningId { get; set; }

    [Range(1, int.MaxValue)]
    [Column("row_index")]
    public int Row { get; set; }

    [Range(1, int.MaxValue)]
    [Column("seat_number")]
    public int Seat { get; set; }

    [Range(1, int.MaxValue)]
    public int NumberOfSeats { get; set; } = 1;

    [Range(0.01, double.MaxValue)]
    public decimal TotalPrice { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
