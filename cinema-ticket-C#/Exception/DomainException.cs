namespace CinemaTicket.Exception;

public class DomainException : System.Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
