using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CinemaTicket.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CinemaTicket.Security;

public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public string GenerateToken(User user, out DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
