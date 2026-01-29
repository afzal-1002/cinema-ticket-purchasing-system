using System.ComponentModel.DataAnnotations;
using System;

namespace CinemaTicket.Models.Entities
{
    public class SeatMapView
    {
        public int ScreeningId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalRows { get; set; }
        public int SeatsPerRow { get; set; }
        public List<string> ReservedSeats { get; set; } = new List<string>();
        public int UserId { get; set; }
    }
}