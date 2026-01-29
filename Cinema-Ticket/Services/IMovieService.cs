using CinemaTicket.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CinemaTicket.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<string>> GetAllMovieTitlesAsync();
        Task<IEnumerable<Screening>> GetScreeningsByMovieTitleAsync(string movieTitle);
        Task<IEnumerable<Screening>> GetScreeningsByTitleAsync(string title);  // Add alias/overload
        Task<Screening?> GetScreeningByIdAsync(int id);
    }
}
