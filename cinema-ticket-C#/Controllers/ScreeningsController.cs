using CinemaTicket.Dto.Screening;
using CinemaTicket.Mapper;
using CinemaTicket.Security;
using CinemaTicket.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicket.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningsController(IScreeningService screeningService) : ControllerBase
{
    private readonly IScreeningService _screeningService = screeningService;

    [HttpGet]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<IReadOnlyCollection<ScreeningSummaryDto>>> List(CancellationToken cancellationToken)
    {
        var screenings = await _screeningService.ListUpcomingAsync(cancellationToken);
        return Ok(screenings.Select(s => s.ToSummary()).ToList());
    }

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ScreeningDetailDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var screening = await _screeningService.GetByIdAsync(id, cancellationToken);
        return Ok(screening.ToDetail());
    }

    [HttpGet("upcoming")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<ScreeningSummaryDto>>> Upcoming(CancellationToken cancellationToken)
    {
        var screenings = await _screeningService.ListUpcomingAsync(cancellationToken);
        return Ok(screenings.Select(s => s.ToSummary()).ToList());
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<ScreeningSummaryDto>> Create([FromBody] CreateScreeningRequest request, CancellationToken cancellationToken)
    {
        var screening = await _screeningService.CreateAsync(request, cancellationToken);
        return Ok(screening.ToSummary());
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await _screeningService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
