using Microsoft.AspNetCore.Identity;

namespace CinemaTicket.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<Model.User> _inner = new();

    public string Hash(string password)
    {
        return _inner.HashPassword(null!, password);
    }

    public bool Verify(string hash, string password)
    {
        var result = _inner.VerifyHashedPassword(null!, hash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
