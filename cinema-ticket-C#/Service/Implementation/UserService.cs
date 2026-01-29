using CinemaTicket.Config;
using CinemaTicket.Dto.Auth;
using CinemaTicket.Dto.User;
using CinemaTicket.Exception;
using CinemaTicket.Mapper;
using CinemaTicket.Model;
using CinemaTicket.Security;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Service.Implementation;

public class UserService(ApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    : IUserService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var existing = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (existing)
        {
            throw new DomainException("Email is already registered.");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsAdmin = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user, out var expiresAt);
        return new AuthResponse(token, expiresAt, user.Email, $"{user.FirstName} {user.LastName}", user.IsAdmin);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken)
                   ?? throw new DomainException("Invalid credentials.");

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new DomainException("Invalid credentials.");
        }

        var token = _jwtTokenService.GenerateToken(user, out var expiresAt);
        return new AuthResponse(token, expiresAt, user.Email, $"{user.FirstName} {user.LastName}", user.IsAdmin);
    }

    public async Task<UserSummaryDto> UpdateAsync(long userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                   ?? throw new DomainException("User not found.");

        _context.Entry(user).Property(u => u.RowVersion).OriginalValue = request.RowVersion;

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException("User was modified by someone else. Please reload and retry.");
        }

        return user.ToSummary();
    }

    public async Task<IReadOnlyCollection<UserSummaryDto>> ListAsync(CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        return users.Select(u => u.ToSummary()).ToList();
    }

    public async Task<User> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
               ?? throw new DomainException("User not found.");
    }
}
