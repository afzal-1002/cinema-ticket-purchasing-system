using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaTicket.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Profile/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            // ✅ Simple: Use default user ID
            int userId = 1; // Default user - no session needed

            // ✅ Get all reservations for the current user
            var reservations = await _context.Reservations
                .Include(r => r.Screening)
                    .ThenInclude(s => s!.Cinema)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            // ✅ Separate active and past bookings
            var now = DateTime.Now;
            ViewBag.ActiveBookings = reservations
                .Where(r => r.Screening != null && r.Screening.StartTime > now)
                .ToList();
            
            ViewBag.PastBookings = reservations
                .Where(r => r.Screening != null && r.Screening.StartTime <= now)
                .ToList();

            return View(reservations);
        }

        // POST: /Profile/CancelBooking/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            // ✅ Simple: Use default user ID
            int userId = 1; // Default user - no session needed

            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Screening)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reservation == null)
                {
                    TempData["ErrorMessage"] = "❌ Booking not found.";
                    return RedirectToAction(nameof(MyBookings));
                }

                // ✅ Verify ownership (optional since we're using default user)
                if (reservation.UserId != userId)
                {
                    TempData["ErrorMessage"] = "❌ You can only cancel your own bookings.";
                    return RedirectToAction(nameof(MyBookings));
                }

                // ✅ Check if screening has already started
                if (reservation.Screening != null && reservation.Screening.StartTime <= DateTime.Now)
                {
                    TempData["ErrorMessage"] = "❌ Cannot cancel booking for a screening that has already started.";
                    return RedirectToAction(nameof(MyBookings));
                }

                // ✅ Delete reservation
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Booking cancelled successfully! Seat {reservation.SeatNumber} is now available.";
                return RedirectToAction(nameof(MyBookings));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error: {ex.Message}";
                return RedirectToAction(nameof(MyBookings));
            }
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            // ✅ FIXED: Use GetString instead of non-existent IsUserLoggedIn()
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["ErrorMessage"] = "❌ Please log in to edit your profile.";
                return RedirectToAction("Login", "Users");
            }

            // ✅ FIXED: Add validation for corrupted session data
            if (!int.TryParse(userIdString, out int userId))
            {
                Console.WriteLine($"ERROR: Invalid UserId in session: '{userIdString}' (corrupted session data)");
                HttpContext.Session.Clear();
                TempData["ErrorMessage"] = "❌ Session expired or corrupted. Please log in again.";
                return RedirectToAction("Login", "Users");
            }

            if (userId != user.Id)
            {
                TempData["Error"] = "Unauthorized action.";
                return RedirectToAction(nameof(MyBookings));  // ✅ Changed from "Index" to "MyBookings"
            }

            // ✅ Remove ModelState validation that might block password field
            ModelState.Remove("Password"); // Password not required for profile update

            if (!ModelState.IsValid)
            {
                return View("Edit", user);
            }

            try
            {
                // ✅ Fetch existing user from DB to preserve password
                var existingUser = await _context.Users.FindAsync(userId);
                if (existingUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(MyBookings));
                }

                // ✅ Update only editable fields
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(MyBookings));
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "Update failed. Please try again.";
                return RedirectToAction(nameof(MyBookings));
            }
        }
    }
}