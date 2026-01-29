using CinemaTicket.Model;

namespace CinemaTicket.Security;

public interface IJwtTokenService
{
    string GenerateToken(User user, out DateTime expiresAt);
}
