using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using CinemaTicket.Services;
using CinemaTicket.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReservationService _reservationService;

        public ReservationsController(
            ApplicationDbContext context,
            IReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        // ✅ GET: /Reservations or /Reservations/Index
        public async Task<IActionResult> Index()
        {
            // ✅ Only admins can view all reservations
            var isAdminString = HttpContext.Session.GetString("IsAdmin");
            if (isAdminString != "True")
            {
                TempData["ErrorMessage"] = "❌ Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            // ✅ Get all reservations with related data
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Screening)
                    .ThenInclude(s => s!.Cinema)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View("Reservation-View", reservations);
        }

        // GET: /Reservations/Create
        public IActionResult Create()
        {
            ViewBag.Screenings = _context.Screenings.ToList();
            return View();
        }

        // POST: /Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reservation model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            model.UserId = userId.Value;
            _context.Reservations.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "❌ Please login first.";
                return RedirectToAction("Login", "Users");
            }

            if (id == null) return BadRequest();

            var reservation = await _context.Reservations
                .Include(r => r.Screening!)
                    .ThenInclude(s => s.Cinema)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "❌ Reservation not found.";
                return RedirectToAction("Index", "Home");
            }

            // Verify ownership
            if (reservation.UserId != userId)
            {
                TempData["ErrorMessage"] = "❌ Access denied.";
                return RedirectToAction("Index", "Home");
            }

            // ✅ Convert RowVersion to Base64
            ViewBag.RowVersion = Convert.ToBase64String(reservation.RowVersion);
            ViewBag.CurrentUsername = HttpContext.Session.GetString("Username");

            return View("Reservation-Edit", reservation);
        }

        // POST: /Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation reservationInput, string rowVersionBase64)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "❌ Please login first.";
                return RedirectToAction("Login", "Users");
            }

            if (id != reservationInput.Id) return BadRequest();

            // Verify ownership
            if (reservationInput.UserId != userId)
            {
                TempData["ErrorMessage"] = "❌ Access denied.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RowVersion = rowVersionBase64;
                ViewBag.CurrentUsername = HttpContext.Session.GetString("Username");
                return View("Reservation-Edit", reservationInput);
            }

            try
            {
                // ✅ Restore RowVersion from hidden field
                reservationInput.RowVersion = Convert.FromBase64String(rowVersionBase64);

                _context.Reservations.Update(reservationInput);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Reservation for seat {reservationInput.SeatNumber} updated successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // ✅ CONCURRENCY CONFLICT DETECTED
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();

                if (databaseValues == null)
                {
                    ViewBag.ReservationDeleted = true;
                    ViewBag.DatabaseValues = null;
                    ViewBag.ClientValues = reservationInput;
                    ViewBag.DatabaseTimestamp = "N/A";
                    ViewBag.ClientTimestamp = rowVersionBase64;
                    ViewBag.ConflictTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ViewBag.CurrentUsername = HttpContext.Session.GetString("Username");

                    return View("Reservation-Edit-Conflict", reservationInput);
                }
                else
                {
                    var databaseReservation = (Reservation)databaseValues.ToObject();
                    
                    ViewBag.DatabaseValues = databaseReservation;
                    ViewBag.ClientValues = reservationInput;
                    ViewBag.DatabaseTimestamp = Convert.ToBase64String(databaseReservation.RowVersion);
                    ViewBag.ClientTimestamp = rowVersionBase64;
                    ViewBag.ConflictTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ViewBag.CurrentUsername = HttpContext.Session.GetString("Username");
                    ViewBag.ReservationDeleted = false;

                    return View("Reservation-Edit-Conflict", reservationInput);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"❌ Error updating reservation: {ex.Message}");
                ViewBag.RowVersion = rowVersionBase64;
                ViewBag.CurrentUsername = HttpContext.Session.GetString("Username");
                return View("Reservation-Edit", reservationInput);
            }
        }

        // POST: /Reservations/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var reservation = _context.Reservations.Find(id);
            if (reservation == null)
                return NotFound();
            _context.Reservations.Remove(reservation);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Reservations/SeatMap/14
        public async Task<IActionResult> SeatMap(int id)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .Include(s => s.Reservations!)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null)
            {
                TempData["ErrorMessage"] = "Screening not found.";
                return RedirectToAction("Index", "Screenings");
            }

            // Get occupied seats
            var occupiedSeats = await _reservationService.GetReservedSeatsAsync(id);
            
            // Convert seat numbers to codes (e.g., 1 -> "A1", 16 -> "B1")
            var reservedSeats = new List<string>();
            if (screening.Cinema != null)
            {
                reservedSeats = occupiedSeats.Select(seatNum => ConvertSeatNumberToCode(seatNum, screening.Cinema)).ToList();
            }

            var viewModel = new SeatMapView
            {
                ScreeningId = screening.Id,
                MovieTitle = screening.MovieTitle,
                CinemaName = screening.Cinema?.Name ?? "Unknown Cinema",
                StartTime = screening.StartTime,
                EndTime = screening.EndTime,
                TotalRows = screening.Cinema?.TotalRows ?? 10,
                SeatsPerRow = screening.Cinema?.SeatsPerRow ?? 15,
                ReservedSeats = reservedSeats,
                UserId = 1 // Default user ID - no session needed
            };

            return View(viewModel);
        }

        // POST: /Reservations/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int screeningId, string selectedSeat)
        {
            // ✅ Simple: Use default user ID
            int userId = 1; // Default user - no session needed

            // ✅ Parse seat number safely
            if (!int.TryParse(selectedSeat, out int seatNumber))
            {
                TempData["ErrorMessage"] = "❌ Invalid seat selection.";
                return RedirectToAction("SeatMap", new { id = screeningId });
            }

            try
            {
                // ✅ Create reservation
                var reservation = new Reservation
                {
                    UserId = userId,
                    ScreeningId = screeningId,
                    SeatNumber = seatNumber,
                    ReservationDate = DateTime.Now,
                    IsPaid = false
                };

                var success = await _reservationService.CreateReservationAsync(reservation);

                if (success)
                {
                    TempData["SuccessMessage"] = $"✅ Seat {seatNumber} reserved successfully!";
                    return RedirectToAction("MyBookings", "Profile");
                }
                else
                {
                    TempData["ErrorMessage"] = "❌ Failed to reserve seat. It may already be taken.";
                    return RedirectToAction("SeatMap", new { id = screeningId });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Booking failed: {ex.Message}";
                return RedirectToAction("SeatMap", new { id = screeningId });
            }
        }

        // POST: /Reservations/ReserveMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReserveMultiple(int screeningId, string selectedSeats)
        {
            // ✅ Simple: Use default user ID
            int userId = 1; // Default user - no session needed

            // ✅ Parse selected seats (comma-separated string like "5,27,48")
            if (string.IsNullOrWhiteSpace(selectedSeats))
            {
                TempData["ErrorMessage"] = "❌ Please select at least one seat.";
                return RedirectToAction("SeatMap", new { id = screeningId });
            }

            var seatNumberStrings = selectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var seatNumbers = new List<int>();

            foreach (var seatStr in seatNumberStrings)
            {
                if (int.TryParse(seatStr.Trim(), out int seatNum))
                {
                    seatNumbers.Add(seatNum);
                }
                else
                {
                    TempData["ErrorMessage"] = $"❌ Invalid seat selection: '{seatStr}'";
                    return RedirectToAction("SeatMap", new { id = screeningId });
                }
            }

            if (seatNumbers.Count == 0)
            {
                TempData["ErrorMessage"] = "❌ Please select at least one seat.";
                return RedirectToAction("SeatMap", new { id = screeningId });
            }

            try
            {
                // ✅ Create reservations for all selected seats
                var successCount = 0;
                var failedSeats = new List<int>();

                foreach (var seatNumber in seatNumbers)
                {
                    var reservation = new Reservation
                    {
                        UserId = userId,
                        ScreeningId = screeningId,
                        SeatNumber = seatNumber,
                        ReservationDate = DateTime.Now,
                        IsPaid = false
                    };

                    var success = await _reservationService.CreateReservationAsync(reservation);

                    if (success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failedSeats.Add(seatNumber);
                    }
                }

                // ✅ Report results
                if (successCount > 0 && failedSeats.Count == 0)
                {
                    // All seats reserved successfully
                    TempData["SuccessMessage"] = $"✅ Successfully reserved {successCount} seat(s)!";
                    return RedirectToAction("MyBookings", "Profile");
                }
                else if (successCount > 0 && failedSeats.Count > 0)
                {
                    // Partial success
                    TempData["SuccessMessage"] = $"✅ Reserved {successCount} seat(s).";
                    TempData["ErrorMessage"] = $"❌ Could not reserve {failedSeats.Count} seat(s) (already taken): {string.Join(", ", failedSeats)}";
                    return RedirectToAction("MyBookings", "Profile");
                }
                else
                {
                    // All failed
                    TempData["ErrorMessage"] = $"❌ Could not reserve any seats. They may already be taken.";
                    return RedirectToAction("SeatMap", new { id = screeningId });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Booking failed: {ex.Message}";
                return RedirectToAction("SeatMap", new { id = screeningId });
            }
        }

        // ✅ Helper method to convert seat number to seat code (e.g., 1 -> "A1", 16 -> "B1")
        private string ConvertSeatNumberToCode(int seatNumber, Cinema cinema)
        {
            int seatsPerRow = cinema.SeatsPerRow;
            int row = (seatNumber - 1) / seatsPerRow;
            int seatInRow = (seatNumber - 1) % seatsPerRow + 1;
            char rowLetter = (char)('A' + row);
            return $"{rowLetter}{seatInRow}";
        }
    }
}
