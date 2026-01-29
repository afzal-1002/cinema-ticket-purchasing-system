using System;
using System.Collections.Generic;

namespace CinemaTicket.Models.DTOs
{
    public class MovieDto
    {
        public int ScreeningId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CinemaName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public int AvailableSeats { get; set; }
    }
}