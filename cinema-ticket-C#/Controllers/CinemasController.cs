using CinemaTicket.Model;
using CinemaTicket.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicket.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CinemasController(ICinemaService cinemaService) : ControllerBase
{
    private readonly ICinemaService _cinemaService = cinemaService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<Cinema>>> List(CancellationToken cancellationToken)
    {
        var cinemas = await _cinemaService.ListAsync(cancellationToken);
        return Ok(cinemas);
    }
}
