using System;
using System.Collections.Generic;

namespace CinemaTicket.Models.DTOs
{
    public class ScreeningDto
    {
        public int Id { get; set; }
        public int CinemaId { get; set; }
        public string? MovieTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? CinemaName { get; set; }
        public string? RoomNumber { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
    }
}
