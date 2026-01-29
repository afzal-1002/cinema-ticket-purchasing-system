using System.Security.Claims;
using CinemaTicket.Dto.User;
using CinemaTicket.Exception;
using CinemaTicket.Mapper;
using CinemaTicket.Security;
using CinemaTicket.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicket.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("me")]
    public async Task<ActionResult<UserSummaryDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(GetUserId(), cancellationToken);
        return Ok(user.ToSummary());
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserSummaryDto>> UpdateCurrentUser([FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var updated = await _userService.UpdateAsync(GetUserId(), request, cancellationToken);
        return Ok(updated);
    }

    [HttpGet]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<IReadOnlyCollection<UserSummaryDto>>> ListUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.ListAsync(cancellationToken);
        return Ok(users);
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
