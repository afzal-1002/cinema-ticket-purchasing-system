using System;
using System.Collections.Generic;

namespace CinemaTicket.Models.DTOs
{
    public class CinemaDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? RoomNumber { get; set; }
        public int TotalRows { get; set; }
        public int SeatsPerRow { get; set; }

        // Added to satisfy CinemaService mapping
        public int TotalScreenings { get; set; }
        public int UpcomingScreenings { get; set; }

        public IEnumerable<ScreeningDto> Screenings { get; set; } = new List<ScreeningDto>();
    }
}