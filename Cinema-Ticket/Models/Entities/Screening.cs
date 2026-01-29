using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Models.Entities
{
    public class Screening
    {
        public int Id { get; set; }

        [Required]
        public int CinemaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string MovieTitle { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // ✅ Navigation property
        [ForeignKey("CinemaId")]
        public virtual Cinema? Cinema { get; set; }

        // ✅ Collection navigation property
        public virtual ICollection<Reservation>? Reservations { get; set; }
    }
}