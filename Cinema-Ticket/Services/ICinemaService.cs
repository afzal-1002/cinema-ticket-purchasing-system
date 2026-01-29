using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicket.Models.Entities;
using CinemaTicket.Models.DTOs; // FIXED namespace

namespace CinemaTicket.Services
{
    public interface ICinemaService
    {
        // Sync
        IEnumerable<Cinema> GetAll();
        Cinema? GetById(int id);
        void Create(Cinema cinema);
        void Update(Cinema cinema);
        void Delete(int id);
        int GetTotalReservations(int cinemaId);
        int GetTotalRevenue(int cinemaId);

        // Async
        Task<IEnumerable<Cinema>> GetAllAsync();
        Task<Cinema?> GetByIdAsync(int id);
        Task<CinemaDto?> GetCinemaDtoByIdAsync(int id);
        Task<IEnumerable<Screening>> GetUpcomingScreeningsAsync(int cinemaId);
        Task<IEnumerable<MovieDto>> GetMoviesByHallNameAsync(string hallName);
        Task<IEnumerable<MovieDto>> GetMoviesComingInDaysAsync(int days);
        Task<IEnumerable<MovieDto>> GetMoviesForCurrentDayAsync();
        Task<IEnumerable<ScreeningDto>> SearchScreeningsByKeywordAsync(string keyword, int daysAhead);
        Task<Cinema> CreateAsync(Cinema cinema);
        Task UpdateAsync(Cinema cinema);
        Task DeleteAsync(int id);

        // Async
        Task<Cinema?> GetCinemaByIdAsync(int id);
        Task<IEnumerable<Cinema>> GetAllCinemasAsync();
        Task<bool> CreateCinemaAsync(Cinema cinema);
        Task<bool> UpdateCinemaAsync(Cinema cinema, byte[] originalRowVersion);
        Task<bool> DeleteCinemaAsync(int id);
        Task<bool> CinemaNameExistsAsync(string name, int? excludeId = null);
    }
}
