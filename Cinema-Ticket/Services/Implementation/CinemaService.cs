using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using CinemaTicket.Models;
using CinemaTicket.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Services.Implementation
{
    public class CinemaService : ICinemaService
    {
        private readonly ApplicationDbContext _context;

        public CinemaService(ApplicationDbContext context) => _context = context;

        // Sync
        public IEnumerable<Cinema> GetAll() =>
            _context.Cinemas.Include(c => c.Screenings).AsNoTracking().ToList();

        public Cinema? GetById(int id) =>
            _context.Cinemas.Include(c => c.Screenings).AsNoTracking().FirstOrDefault(c => c.Id == id);

        public void Create(Cinema cinema)
        {
            _context.Cinemas.Add(cinema);
            _context.SaveChanges();
        }

        public void Update(Cinema cinema)
        {
            _context.Cinemas.Update(cinema);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Cinemas.Find(id);
            if (entity == null) return;
            _context.Cinemas.Remove(entity);
            _context.SaveChanges();
        }

        public int GetTotalReservations(int cinemaId) =>
            _context.Reservations.Count(r => r.Screening != null && r.Screening.CinemaId == cinemaId);

        public int GetTotalRevenue(int cinemaId)
        {
            const decimal ticketPrice = 10m;
            return (int)(GetTotalReservations(cinemaId) * ticketPrice);
        }

        // Async
        public async Task<IEnumerable<Cinema>> GetAllAsync() =>
            await _context.Cinemas.Include(c => c.Screenings).AsNoTracking().ToListAsync();

        public async Task<Cinema?> GetByIdAsync(int id) =>
            await _context.Cinemas.Include(c => c.Screenings).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        public async Task<CinemaDto?> GetCinemaDtoByIdAsync(int id)
        {
            var cinema = await _context.Cinemas.Include(c => c.Screenings).FirstOrDefaultAsync(c => c.Id == id);
            if (cinema == null) return null;
            return new CinemaDto
            {
                Id = cinema.Id,
                Name = cinema.Name,
                RoomNumber = cinema.RoomNumber,
                TotalRows = cinema.TotalRows,
                SeatsPerRow = cinema.SeatsPerRow,
                TotalScreenings = cinema.Screenings?.Count ?? 0,
                UpcomingScreenings = cinema.Screenings?.Count(s => s.StartTime > DateTime.Now) ?? 0
            };
        }

        public async Task<IEnumerable<Screening>> GetUpcomingScreeningsAsync(int cinemaId) =>
            await _context.Screenings.Include(s => s.Cinema)
                .Where(s => s.CinemaId == cinemaId && s.StartTime > DateTime.Now)
                .OrderBy(s => s.StartTime)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<MovieDto>> GetMoviesByHallNameAsync(string hallName)
        {
            var screenings = await _context.Screenings.Include(s => s.Cinema)
                .Where(s => s.Cinema != null && s.Cinema.Name.Contains(hallName))
                .OrderBy(s => s.StartTime)
                .AsNoTracking()
                .ToListAsync();

            return screenings.Select(MapToMovieDto).ToList();
        }

        public async Task<IEnumerable<MovieDto>> GetMoviesComingInDaysAsync(int days)
        {
            var start = DateTime.Now;
            var end = start.AddDays(days);
            var screenings = await _context.Screenings.Include(s => s.Cinema)
                .Where(s => s.StartTime >= start && s.StartTime <= end)
                .OrderBy(s => s.StartTime)
                .AsNoTracking()
                .ToListAsync();
            return screenings.Select(MapToMovieDto).ToList();
        }

        public async Task<IEnumerable<MovieDto>> GetMoviesForCurrentDayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var screenings = await _context.Screenings.Include(s => s.Cinema)
                .Where(s => s.StartTime >= today && s.StartTime < tomorrow)
                .OrderBy(s => s.StartTime)
                .AsNoTracking()
                .ToListAsync();
            return screenings.Select(MapToMovieDto).ToList();
        }

        public async Task<IEnumerable<ScreeningDto>> SearchScreeningsByKeywordAsync(string keyword, int daysAhead)
        {
            var start = DateTime.Now;
            var end = start.AddDays(daysAhead);
            var list = await _context.Screenings.Include(s => s.Cinema)
                .Where(s => s.MovieTitle.Contains(keyword) &&
                            s.StartTime >= start &&
                            s.StartTime <= end)
                .OrderBy(s => s.StartTime)
                .AsNoTracking()
                .ToListAsync();
            return list.Select(MapToScreeningDto).ToList();
        }

        public async Task<Cinema> CreateAsync(Cinema cinema)
        {
            _context.Cinemas.Add(cinema);
            await _context.SaveChangesAsync();
            return cinema;
        }

        public async Task UpdateAsync(Cinema cinema)
        {
            _context.Cinemas.Update(cinema);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Cinemas.FindAsync(id);
            if (entity == null) return;
            _context.Cinemas.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Cinema?> GetCinemaByIdAsync(int id)
        {
            return await _context.Cinemas.FindAsync(id);
        }

        public async Task<IEnumerable<Cinema>> GetAllCinemasAsync()
        {
            return await _context.Cinemas.ToListAsync();
        }

        public async Task<bool> CreateCinemaAsync(Cinema cinema)
        {
            try
            {
                _context.Cinemas.Add(cinema);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ✅ CONCURRENCY-AWARE UPDATE
        public async Task<bool> UpdateCinemaAsync(Cinema cinema, byte[] originalRowVersion)
        {
            try
            {
                // ✅ Restore original timestamp before update
                cinema.RowVersion = originalRowVersion;

                _context.Cinemas.Update(cinema);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // ✅ Concurrency conflict detected
                return false;
            }
        }

        public async Task<bool> DeleteCinemaAsync(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null) return false;

            _context.Cinemas.Remove(cinema);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CinemaNameExistsAsync(string name, int? excludeId = null)
        {
            return await _context.Cinemas
                .AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId));
        }

        // Helpers
        private MovieDto MapToMovieDto(Screening s)
        {
            var totalSeats = (s.Cinema?.TotalRows ?? 0) * (s.Cinema?.SeatsPerRow ?? 0);
            var reserved = _context.Reservations.Count(r => r.ScreeningId == s.Id);
            return new MovieDto
            {
                ScreeningId = s.Id,
                MovieTitle = s.MovieTitle,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                CinemaName = s.Cinema?.Name ?? "Unknown",
                RoomNumber = s.Cinema?.RoomNumber ?? "Unknown",
                AvailableSeats = totalSeats - reserved
            };
        }

        private ScreeningDto MapToScreeningDto(Screening s)
        {
            var totalSeats = (s.Cinema?.TotalRows ?? 0) * (s.Cinema?.SeatsPerRow ?? 0);
            var reserved = _context.Reservations.Count(r => r.ScreeningId == s.Id);
            return new ScreeningDto
            {
                Id = s.Id,
                CinemaId = s.CinemaId,
                MovieTitle = s.MovieTitle,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                CinemaName = s.Cinema?.Name ?? "Unknown",
                RoomNumber = s.Cinema?.RoomNumber ?? "Unknown",
                TotalSeats = totalSeats,
                AvailableSeats = totalSeats - reserved
            };
        }
    }
}
