using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Model;

[Table("users")]
public class User
{
    public long Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
