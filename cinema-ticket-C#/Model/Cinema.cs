using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Model;

[Table("cinemas")]
public class Cinema
{
    public long Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Column("row_count")]
    public int Rows { get; set; }

    [Range(1, int.MaxValue)]
    public int SeatsPerRow { get; set; }

    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
