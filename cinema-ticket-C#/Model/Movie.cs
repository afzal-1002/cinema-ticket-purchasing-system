using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Model;

[Table("movies")]
public class Movie
{
    public long Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int DurationMinutes { get; set; }

    [Required, MaxLength(100)]
    public string Genre { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Rating { get; set; }

    [MaxLength(100)]
    public string? Director { get; set; }

    [MaxLength(500)]
    public string? Cast { get; set; }

    [MaxLength(500)]
    public string? PosterUrl { get; set; }

    [MaxLength(500)]
    public string? TrailerUrl { get; set; }

    public DateOnly ReleaseDate { get; set; }

    [Range(0, 10)]
    [Column(TypeName = "decimal(3,1)")]
    public decimal? AverageRating { get; set; }

    [Range(0, int.MaxValue)]
    public int TotalReviews { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
