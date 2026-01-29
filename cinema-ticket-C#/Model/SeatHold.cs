using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Model;

[Table("seat_holds")]
public class SeatHold
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime HeldAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
