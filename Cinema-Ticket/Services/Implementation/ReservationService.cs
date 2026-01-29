using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Services.Implementation
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation?> GetReservationByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Screening)
                    .ThenInclude(s => s!.Cinema)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserIdAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.Screening)
                    .ThenInclude(s => s!.Cinema)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByScreeningIdAsync(int screeningId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Where(r => r.ScreeningId == screeningId)
                .OrderBy(r => r.SeatNumber)
                .ToListAsync();
        }

        public async Task<bool> CreateReservationAsync(Reservation reservation)
        {
            try
            {
                // ✅ Check if seat is already reserved (first line of defense)
                if (await IsSeatReservedAsync(reservation.ScreeningId, reservation.SeatNumber))
                {
                    return false;
                }

                // ✅ Try to save the reservation
                // The database will enforce the unique constraint on (ScreeningId, SeatNumber)
                // If another user books the same seat at the same time, database will throw exception
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                // ✅ Database threw exception - unique constraint violation
                // This happens when two users try to book the same seat at the same time
                return false;
            }
        }

        public async Task<bool> CancelReservationAsync(int reservationId, int userId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
            {
                return false;
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsSeatAvailableAsync(int screeningId, int seatNumber)
        {
            return !await IsSeatReservedAsync(screeningId, seatNumber);
        }

        public async Task<int> GetAvailableSeatsCountAsync(int screeningId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null) return 0;

            // ✅ FIXED: Use Capacity property (computed from TotalRows * SeatsPerRow)
            var totalSeats = screening.Cinema?.Capacity ?? 0;
            var reservedSeatsCount = await _context.Reservations
                .CountAsync(r => r.ScreeningId == screeningId);

            return totalSeats - reservedSeatsCount;
        }

        public async Task<Dictionary<int, bool>> GetSeatMapAsync(int screeningId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null)
            {
                return new Dictionary<int, bool>();
            }

            // ✅ FIXED: Use Capacity property
            var totalSeats = screening.Cinema?.Capacity ?? 0;
            var reservedSeats = await GetReservedSeatsAsync(screeningId);

            var seatMap = new Dictionary<int, bool>();
            for (int i = 1; i <= totalSeats; i++)
            {
                seatMap[i] = !reservedSeats.Contains(i);
            }

            return seatMap;
        }

        // ✅ EXISTING: UpdateReservationAsync(Reservation) - kept for backward compatibility
        public async Task<bool> UpdateReservationAsync(Reservation reservation)
        {
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservation.Id);

            if (existingReservation == null)
            {
                return false;
            }

            existingReservation.SeatNumber = reservation.SeatNumber;
            existingReservation.IsPaid = reservation.IsPaid;

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ NEW: UpdateReservationAsync(Reservation, byte[]) - interface requirement
        public async Task<bool> UpdateReservationAsync(Reservation reservation, byte[] rowVersion)
        {
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservation.Id);

            if (existingReservation == null)
            {
                return false;
            }

            // ✅ Optimistic concurrency check (RowVersion)
            if (!existingReservation.RowVersion.SequenceEqual(rowVersion))
            {
                return false; // Concurrent modification detected
            }

            existingReservation.SeatNumber = reservation.SeatNumber;
            existingReservation.IsPaid = reservation.IsPaid;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        // ✅ NEW: DeleteReservationAsync(int)
        public async Task<bool> DeleteReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return false;
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ NEW: GetUserReservationsAsync(int)
        public async Task<IEnumerable<Reservation>> GetUserReservationsAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.Screening)
                    .ThenInclude(s => s!.Cinema)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        // ✅ NEW: GetOccupiedSeatCodes(int) - synchronous method
        public HashSet<string> GetOccupiedSeatCodes(int screeningId)
        {
            var reservedSeats = _context.Reservations
                .Where(r => r.ScreeningId == screeningId)
                .Select(r => r.SeatNumber)
                .ToList();

            // Convert INT seat numbers to string seat codes (e.g., 5 -> "A5")
            var seatCodes = new HashSet<string>();
            foreach (var seatNum in reservedSeats)
            {
                seatCodes.Add(ConvertSeatNumberToCode(seatNum));
            }

            return seatCodes;
        }

        // ✅ NEW: GetOccupiedSeatOwners(int) - synchronous method
        public Dictionary<string, string> GetOccupiedSeatOwners(int screeningId)
        {
            var reservations = _context.Reservations
                .Include(r => r.User)
                .Where(r => r.ScreeningId == screeningId)
                .Select(r => new { r.SeatNumber, r.User!.Username })
                .ToList();

            var seatOwners = new Dictionary<string, string>();
            foreach (var res in reservations)
            {
                var seatCode = ConvertSeatNumberToCode(res.SeatNumber);
                seatOwners[seatCode] = res.Username;
            }

            return seatOwners;
        }

        // ✅ NEW: ReserveSeat(int, int, int, int) - synchronous method
        public async Task<bool> ReserveSeat(int screeningId, int userId, int row, int seat)
        {
            var seatNumber = CalculateSeatNumber(screeningId, row, seat);

            if (await _context.Reservations.AnyAsync(r => r.ScreeningId == screeningId && r.SeatNumber == seatNumber))
            {
                return false; // Seat already reserved
            }

            var reservation = new Reservation
            {
                ScreeningId = screeningId,
                UserId = userId,
                SeatNumber = seatNumber,
                ReservationDate = DateTime.UtcNow,
                IsPaid = false
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ NEW: ReleaseSeat(int, int, int, int, bool) - synchronous method
        public async Task<bool> ReleaseSeat(int screeningId, int userId, int row, int seat, bool forceRelease)
        {
            var seatNumber = CalculateSeatNumber(screeningId, row, seat);

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ScreeningId == screeningId && r.SeatNumber == seatNumber);

            if (reservation == null)
            {
                return false; // Seat not reserved
            }

            // Check ownership unless force release
            if (!forceRelease && reservation.UserId != userId)
            {
                return false; // User doesn't own this reservation
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ HELPER: Get reserved seat numbers as int list
        public async Task<List<int>> GetReservedSeatsAsync(int screeningId)
        {
            return await _context.Reservations
                .Where(r => r.ScreeningId == screeningId)
                .Select(r => r.SeatNumber)
                .ToListAsync();
        }

        // ✅ HELPER: Check if specific seat is reserved
        public async Task<bool> IsSeatReservedAsync(int screeningId, int seatNumber)
        {
            return await _context.Reservations
                .AnyAsync(r => r.ScreeningId == screeningId && r.SeatNumber == seatNumber);
        }

        // ✅ HELPER: Get available seats as int list
        public async Task<List<int>> GetAvailableSeatsAsync(int screeningId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null) return new List<int>();

            var totalSeats = screening.Cinema?.Capacity ?? 0;
            var reservedSeats = await GetReservedSeatsAsync(screeningId);

            return Enumerable.Range(1, totalSeats)
                .Except(reservedSeats)
                .ToList();
        }

        public async Task<Dictionary<int, int>> GetSeatStatisticsAsync(int screeningId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening == null) return new Dictionary<int, int>();

            var totalSeats = screening.Cinema?.Capacity ?? 0;
            var reservedSeatsCount = await _context.Reservations
                .CountAsync(r => r.ScreeningId == screeningId);

            return new Dictionary<int, int>
            {
                { 1, totalSeats },      // Total seats
                { 2, reservedSeatsCount }, // Reserved seats
                { 3, totalSeats - reservedSeatsCount } // Available seats
            };
        }

        // ✅ HELPER: Reserve specific seat
        public async Task<bool> ReserveSeatAsync(int screeningId, int seatNumber, int userId)
        {
            if (await IsSeatReservedAsync(screeningId, seatNumber))
            {
                return false;
            }

            var reservation = new Reservation
            {
                ScreeningId = screeningId,
                SeatNumber = seatNumber,
                UserId = userId,
                ReservationDate = DateTime.UtcNow,
                IsPaid = false
            };

            return await CreateReservationAsync(reservation);
        }

        // ✅ HELPER: Get user's reservations for specific screening
        public async Task<List<Reservation>> GetUserReservationsForScreeningAsync(int userId, int screeningId)
        {
            return await _context.Reservations
                .Include(r => r.Screening)
                    .ThenInclude(s => s!.Cinema)
                .Where(r => r.UserId == userId && r.ScreeningId == screeningId)
                .ToListAsync();
        }

        // ✅ PRIVATE HELPER: Convert seat number to seat code (e.g., 5 -> "A5")
        private string ConvertSeatNumberToCode(int seatNumber)
        {
            // Assuming 20 seats per row (adjust based on your cinema layout)
            int seatsPerRow = 20;
            int row = (seatNumber - 1) / seatsPerRow;
            int seat = (seatNumber - 1) % seatsPerRow + 1;
            char rowLetter = (char)('A' + row);
            return $"{rowLetter}{seat}";
        }

        // ✅ PRIVATE HELPER: Calculate seat number from row/seat (e.g., row=0, seat=5 -> 5)
        private int CalculateSeatNumber(int screeningId, int row, int seat)
        {
            // Assuming 20 seats per row (adjust based on cinema layout)
            int seatsPerRow = 20;
            return row * seatsPerRow + seat;
        }
    }
}
