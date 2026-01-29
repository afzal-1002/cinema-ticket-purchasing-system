using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaTicket.Services.Implementation
{
    public class ScreeningService : IScreeningService
    {
        private readonly ApplicationDbContext _context;

        public ScreeningService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Screening?> GetScreeningByIdAsync(int id)
        {
            return await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Screening>> GetAllScreeningsAsync()
        {
            return await _context.Screenings
                .Include(s => s.Cinema)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> GetScreeningsByCinemaIdAsync(int cinemaId)
        {
            return await _context.Screenings
                .Include(s => s.Cinema)
                .Where(s => s.CinemaId == cinemaId)
                .ToListAsync();
        }

        public async Task<bool> CreateScreeningAsync(Screening screening)
        {
            try
            {
                _context.Screenings.Add(screening);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ✅ CONCURRENCY-AWARE UPDATE
        public async Task<bool> UpdateScreeningAsync(Screening screening, byte[] originalRowVersion)
        {
            try
            {
                // ✅ Restore original timestamp before update
                screening.RowVersion = originalRowVersion;

                _context.Screenings.Update(screening);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // ✅ Concurrency conflict detected
                return false;
            }
        }

        public async Task<bool> DeleteScreeningAsync(int id)
        {
            var screening = await _context.Screenings.FindAsync(id);
            if (screening == null) return false;

            _context.Screenings.Remove(screening);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}