using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Dto.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);
