namespace CinemaTicket.Dto.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    string Email,
    string FullName,
    bool IsAdmin
);
