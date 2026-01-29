using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicket.Models.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        
        // ✅ FIXED: Explicitly map UserId as foreign key
        [ForeignKey("User")]
        public int UserId { get; set; }
        
        [ForeignKey("Screening")]
        public int ScreeningId { get; set; }
        
        public int SeatNumber { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; }
        
        // Navigation properties
        public User? User { get; set; }
        public Screening? Screening { get; set; }
        
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}

