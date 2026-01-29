using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Model;

[Table("screenings")]
public class Screening
{
    public long Id { get; set; }

    [Required]
    public Cinema Cinema { get; set; } = null!;
    public long CinemaId { get; set; }

    [Required]
    public Movie Movie { get; set; } = null!;
    public long MovieId { get; set; }

    [Required]
    public DateTime StartDateTime { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(typeof(decimal), "0.01", "9999999999.99")]
    public decimal TicketPrice { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
