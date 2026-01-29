using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Models.Entities
{
    public class Cinema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public int TotalRows { get; set; }

        [Required]
        public int SeatsPerRow { get; set; }

        // ✅ ADDED: Computed property for total capacity
        [NotMapped]
        public int Capacity => TotalRows * SeatsPerRow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation properties
        public ICollection<Screening>? Screenings { get; set; }
    }
}