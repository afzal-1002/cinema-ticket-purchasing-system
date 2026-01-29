using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Dto.Screening;

public class CreateScreeningRequest
{
    [Required]
    public long CinemaId { get; set; }

    [Required]
    public long MovieId { get; set; }

    [Required]
    public DateTime StartDateTime { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999.99")]
    public decimal TicketPrice { get; set; }
}
