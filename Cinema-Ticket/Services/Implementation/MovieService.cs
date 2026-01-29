using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using CinemaTicket.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaTicket.Services.Implementation
{
    public class MovieService : IMovieService
    {
        private readonly ApplicationDbContext _context;

        public MovieService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> GetAllMovieTitlesAsync()
        {
            return await _context.Screenings
                .Select(s => s.MovieTitle)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> GetScreeningsByMovieTitleAsync(string movieTitle)
        {
            return await _context.Screenings
                .Include(s => s.Cinema)
                .Where(s => s.MovieTitle == movieTitle)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        // Add alias method to match controller usage
        public async Task<IEnumerable<Screening>> GetScreeningsByTitleAsync(string title)
        {
            return await GetScreeningsByMovieTitleAsync(title);
        }

        public async Task<Screening?> GetScreeningByIdAsync(int id)
        {
            return await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
