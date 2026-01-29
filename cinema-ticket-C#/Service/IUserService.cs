using CinemaTicket.Dto.Auth;
using CinemaTicket.Dto.User;
using CinemaTicket.Model;

namespace CinemaTicket.Service;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<UserSummaryDto> UpdateAsync(long userId, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserSummaryDto>> ListAsync(CancellationToken cancellationToken);
    Task<User> GetByIdAsync(long userId, CancellationToken cancellationToken);
}
