using System.Security.Claims;
using CinemaTicket.Dto.Reservation;
using CinemaTicket.Dto.Seat;
using CinemaTicket.Exception;
using CinemaTicket.Mapper;
using CinemaTicket.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicket.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReservationsController(IReservationService reservationService) : ControllerBase
{
    private readonly IReservationService _reservationService = reservationService;

    [HttpGet("screenings/{screeningId:long}/seat-map")]
    [AllowAnonymous]
    public async Task<ActionResult<SeatMapResponse>> SeatMap(long screeningId, CancellationToken cancellationToken)
    {
        var seatMap = await _reservationService.GetSeatMapAsync(screeningId, cancellationToken);
        return Ok(seatMap);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationSummaryDto>> Reserve([FromBody] SeatReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.ReserveSeatAsync(GetUserId(), request, cancellationToken);
        return Ok(reservation.ToSummary());
    }

    [HttpDelete]
    public async Task<IActionResult> Cancel([FromBody] CancelReservationRequest request, CancellationToken cancellationToken)
    {
        await _reservationService.CancelReservationAsync(GetUserId(), request, cancellationToken);
        return NoContent();
    }

    private long GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(id, out var userId))
        {
            throw new DomainException("User session is not available.");
        }

        return userId;
    }
}
