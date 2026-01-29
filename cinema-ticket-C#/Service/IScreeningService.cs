using CinemaTicket.Dto.Screening;
using CinemaTicket.Model;

namespace CinemaTicket.Service;

public interface IScreeningService
{
    Task<Screening> CreateAsync(CreateScreeningRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(long screeningId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Screening>> ListUpcomingAsync(CancellationToken cancellationToken);
    Task<Screening> GetByIdAsync(long id, CancellationToken cancellationToken);
}
