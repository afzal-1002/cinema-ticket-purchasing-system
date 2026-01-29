using CinemaTicket.Model;

namespace CinemaTicket.Service;

public interface ICinemaService
{
    Task<IReadOnlyCollection<Cinema>> ListAsync(CancellationToken cancellationToken);
}
