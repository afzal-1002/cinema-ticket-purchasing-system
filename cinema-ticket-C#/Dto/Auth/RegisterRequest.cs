using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Dto.Auth;

public record RegisterRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password,
    [property: Required, MaxLength(50)] string FirstName,
    [property: Required, MaxLength(50)] string LastName,
    [property: Required, MaxLength(20)] string PhoneNumber
);
