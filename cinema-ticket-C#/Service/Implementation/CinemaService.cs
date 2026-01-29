using CinemaTicket.Config;
using CinemaTicket.Model;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Service.Implementation;

public class CinemaService(ApplicationDbContext context) : ICinemaService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IReadOnlyCollection<Cinema>> ListAsync(CancellationToken cancellationToken)
    {
        var cinemas = await _context.Cinemas
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return cinemas;
    }
}
