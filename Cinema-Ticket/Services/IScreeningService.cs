using CinemaTicket.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CinemaTicket.Services
{
    public interface IScreeningService
    {
        Task<Screening?> GetScreeningByIdAsync(int id);
        Task<IEnumerable<Screening>> GetAllScreeningsAsync();
        Task<IEnumerable<Screening>> GetScreeningsByCinemaIdAsync(int cinemaId);
        Task<bool> CreateScreeningAsync(Screening screening);
        Task<bool> UpdateScreeningAsync(Screening screening, byte[] originalRowVersion);
        Task<bool> DeleteScreeningAsync(int id);
    }
}