using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicket.Models.Entities;

namespace CinemaTicket.Services
{
    public interface IReservationService
    {
        // ✅ CRUD Operations
        Task<bool> CreateReservationAsync(Reservation reservation);
        Task<bool> UpdateReservationAsync(Reservation reservation);
        Task<bool> UpdateReservationAsync(Reservation reservation, byte[] rowVersion);
        Task<bool> DeleteReservationAsync(int id);
        Task<bool> CancelReservationAsync(int reservationId, int userId);

        // ✅ Query Operations
        Task<Reservation?> GetReservationByIdAsync(int id);
        Task<IEnumerable<Reservation>> GetUserReservationsAsync(int userId);
        Task<IEnumerable<Reservation>> GetReservationsByScreeningIdAsync(int screeningId);
        
        // ✅ Seat Management (Async)
        Task<List<int>> GetReservedSeatsAsync(int screeningId);
        Task<bool> IsSeatReservedAsync(int screeningId, int seatNumber);
        Task<List<int>> GetAvailableSeatsAsync(int screeningId);  // ✅ ADDED (Line 258)
        Task<Dictionary<int, int>> GetSeatStatisticsAsync(int screeningId);  // ✅ ADDED (Line 283)
        
        // ✅ Seat Management (Synchronous - for backward compatibility)
        HashSet<string> GetOccupiedSeatCodes(int screeningId);
        Dictionary<string, string> GetOccupiedSeatOwners(int screeningId);
        
        // ✅ Seat Operations (Row/Seat based)
        Task<bool> ReserveSeat(int screeningId, int userId, int row, int seat);
        Task<bool> ReleaseSeat(int screeningId, int userId, int row, int seat, bool forceRelease);
    }
}
