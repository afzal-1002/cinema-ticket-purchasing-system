namespace CinemaTicket.Security;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "cinema-ticket";
    public string Audience { get; set; } = "cinema-ticket-clients";
    public string SigningKey { get; set; } = "change-me-please";
    public int ExpirationMinutes { get; set; } = 60;
}
