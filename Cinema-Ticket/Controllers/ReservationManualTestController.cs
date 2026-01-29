using System.Security.Claims;
using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using CinemaTicket.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Controllers
{
    public class ReservationManualTestController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly ApplicationDbContext _context;

        public ReservationManualTestController(
            IReservationService reservationService,
            ApplicationDbContext context)
        {
            _reservationService = reservationService;
            _context = context;
        }

        // GET: /ReservationManualTest/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Screening)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            // ✅ Convert RowVersion to Base64 for hidden field in view
            ViewBag.OriginalRowVersion = Convert.ToBase64String(reservation.RowVersion);
            return View(reservation);
        }

        // POST: /ReservationManualTest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string seatNumber, string originalRowVersionBase64)
        {
            try
            {
                var reservation = await _context.Reservations.FindAsync(id);
                if (reservation == null) return NotFound();

                // ✅ Convert Base64 string back to byte[]
                byte[] originalRowVersion = Convert.FromBase64String(originalRowVersionBase64);

                // ✅ FIXED: Line 54 - Parse seatNumber string to int
                if (!int.TryParse(seatNumber, out int seatNumberInt))
                {
                    TempData["ErrorMessage"] = "❌ Invalid seat number format.";
                    return RedirectToAction("Edit", new { id });
                }

                // Update seat number
                reservation.SeatNumber = seatNumberInt;

                // ✅ Call concurrency-aware service method
                var success = await _reservationService.UpdateReservationAsync(reservation, originalRowVersion);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"✅ Update successful! New RowVersion: {Convert.ToBase64String(reservation.RowVersion)}";
                    return RedirectToAction("Edit", new { id });
                }
                else
                {
                    // Concurrency conflict detected
                    TempData["ErrorMessage"] = "❌ Concurrency Error: The reservation was modified by another user. Please refresh and try again.";
                    return RedirectToAction("Edit", new { id });
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                TempData["ErrorMessage"] = $"❌ Concurrency Error: {ex.Message}";
                return RedirectToAction("Edit", new { id });
            }
        }

        // Test action to manually create reservations
        public async Task<IActionResult> CreateTestReservation()
        {
            var user = new User
            {
                Username = "testuser_" + DateTime.Now.Ticks,
                Password = "password123",
                Email = $"test{DateTime.Now.Ticks}@example.com",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "1234567890",
                IsAdmin = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var screenings = await _context.Screenings.FirstOrDefaultAsync();
            if (screenings != null)
            {
                // ✅ FIXED: Line 103 - Use int instead of string for SeatNumber
                var reservation = new Reservation
                {
                    UserId = user.Id,
                    ScreeningId = screenings.Id,
                    SeatNumber = 1  // ✅ CHANGED: "A1" -> 1 (int)
                };

                var success = await _reservationService.CreateReservationAsync(reservation);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"✅ Test user created (ID: {user.Id}) with reservation for seat 1";
                }
                else
                {
                    TempData["ErrorMessage"] = "❌ User created but reservation failed";
                }
            }
            else
            {
                TempData["SuccessMessage"] = $"✅ Test user created (ID: {user.Id}). No screenings available for reservation.";
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /ReservationManualTest/TestSeatReservation?screeningId=1&row=0&seat=5
        public async Task<IActionResult> TestSeatReservation(int screeningId, int row, int seat)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Users");
            }

            // ✅ FIXED: Line 57 - Parse UserId claim to int
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid user ID. Please login again."
                });
            }

            try
            {
                var success = await _reservationService.ReserveSeat(screeningId, userId, row, seat);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Seat reserved successfully! (Screening: {screeningId}, Row: {row}, Seat: {seat})"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Seat is already reserved or screening not found."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: /ReservationManualTest/TestReleaseSeat?screeningId=1&row=0&seat=5&forceRelease=false
        public async Task<IActionResult> TestReleaseSeat(int screeningId, int row, int seat, bool forceRelease = false)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Users");
            }

            // ✅ FIXED: Line 106 - Parse UserId claim to int
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid user ID. Please login again."
                });
            }

            try
            {
                var success = await _reservationService.ReleaseSeat(screeningId, userId, row, seat, forceRelease);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Seat released successfully! (Screening: {screeningId}, Row: {row}, Seat: {seat})"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = forceRelease 
                        ? "Seat not found or already released." 
                        : "Seat not found, already released, or you don't own this reservation."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: /ReservationManualTest/TestGetReservedSeats?screeningId=1
        public async Task<IActionResult> TestGetReservedSeats(int screeningId)
        {
            try
            {
                var reservedSeats = await _reservationService.GetReservedSeatsAsync(screeningId);

                return Ok(new
                {
                    success = true,
                    screeningId,
                    totalReserved = reservedSeats.Count,
                    seatNumbers = reservedSeats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: /ReservationManualTest/TestGetAvailableSeats?screeningId=1
        public async Task<IActionResult> TestGetAvailableSeats(int screeningId)
        {
            try
            {
                var availableSeats = await _reservationService.GetAvailableSeatsAsync(screeningId);

                return Ok(new
                {
                    success = true,
                    screeningId,
                    totalAvailable = availableSeats.Count,
                    seatNumbers = availableSeats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: /ReservationManualTest/TestSeatStatistics?screeningId=1
        public async Task<IActionResult> TestSeatStatistics(int screeningId)
        {
            try
            {
                var stats = await _reservationService.GetSeatStatisticsAsync(screeningId);

                return Ok(new
                {
                    success = true,
                    screeningId,
                    totalSeats = stats[1],
                    reservedSeats = stats[2],
                    availableSeats = stats[3]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
    }
}