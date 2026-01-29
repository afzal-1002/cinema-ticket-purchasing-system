using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using CinemaTicket.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaTicket.Controllers
{
    public class CinemaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICinemaService _cinemaService;

        public CinemaController(ApplicationDbContext context, ICinemaService cinemaService)
        {
            _context = context;
            _cinemaService = cinemaService;
        }

        // GET: /Cinema or /Cinema/Index
        public async Task<IActionResult> Index()
        {
            // ✅ CHECK IF USER IS LOGGED IN
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["ErrorMessage"] = "❌ Please log in to view cinemas.";
                return RedirectToAction("Login", "Users");
            }

            var cinemas = await _cinemaService.GetAllCinemasAsync();
            
            // ✅ Pass admin status to view
            ViewBag.IsAdmin = HttpContext.Session.GetString("UserRole") == "Admin";
            
            return View("Cinema-Index", cinemas);
        }

        // GET: /Cinema/Create
        public IActionResult Create()
        {
            // ✅ ADMIN-ONLY CHECK
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "❌ Access denied. Only administrators can create cinemas.";
                return RedirectToAction(nameof(Index));
            }

            return View("Cinema-Create");
        }

        // POST: /Cinema/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cinema cinema)
        {
            if (!ModelState.IsValid)
            {
                return View("Cinema-Create", cinema);
            }

            try
            {
                var success = await _cinemaService.CreateCinemaAsync(cinema);
                if (success)
                {
                    TempData["SuccessMessage"] = $"✅ Cinema '{cinema.Name}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "❌ Failed to create cinema.";
                return View("Cinema-Create", cinema);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error: {ex.Message}";
                return View("Cinema-Create", cinema);
            }
        }

        // GET: /Cinema/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                TempData["ErrorMessage"] = "❌ Cinema not found.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Pass RowVersion as Base64
            ViewBag.OriginalRowVersion = Convert.ToBase64String(cinema.RowVersion);
            
            return View("Cinema-Edit", cinema);
        }

        // POST: /Cinema/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string name,
            string roomNumber,
            int totalRows,
            int seatsPerRow,
            string originalRowVersionBase64,
            bool forceOverride = false)
        {
            try
            {
                if (string.IsNullOrEmpty(originalRowVersionBase64))
                {
                    TempData["ErrorMessage"] = "❌ Missing concurrency token. Please reload the page.";
                    return RedirectToAction("Edit", new { id });
                }

                var cinema = await _context.Cinemas.FindAsync(id);
                if (cinema == null)
                {
                    TempData["ErrorMessage"] = "❌ Cinema not found.";
                    return RedirectToAction(nameof(Index));
                }

                byte[] originalRowVersion = Convert.FromBase64String(originalRowVersionBase64);

                if (forceOverride)
                {
                    await _context.Entry(cinema).ReloadAsync();
                    
                    cinema.Name = name;
                    cinema.RoomNumber = roomNumber;
                    cinema.TotalRows = totalRows;
                    cinema.SeatsPerRow = seatsPerRow;

                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "✅ Cinema updated successfully (overridden).";
                    return RedirectToAction(nameof(Index));
                }

                if (!cinema.RowVersion.SequenceEqual(originalRowVersion))
                {
                    var conflictModel = new Cinema
                    {
                        Id = id,
                        Name = name,
                        RoomNumber = roomNumber,
                        TotalRows = totalRows,
                        SeatsPerRow = seatsPerRow
                    };
                    
                    return View("Cinema-Edit-Conflict", conflictModel);
                }

                cinema.Name = name;
                cinema.RoomNumber = roomNumber;
                cinema.TotalRows = totalRows;
                cinema.SeatsPerRow = seatsPerRow;

                _context.Entry(cinema).Property(c => c.RowVersion).OriginalValue = originalRowVersion;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Cinema '{cinema.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "❌ Concurrency conflict detected.";
                return RedirectToAction("Edit", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error: {ex.Message}";
                return RedirectToAction("Edit", new { id });
            }
        }

        // GET: /Cinema/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // ✅ Include Screenings AND Reservations for capacity calculation
            var cinema = await _context.Cinemas
                .Include(c => c.Screenings!)
                    .ThenInclude(s => s.Reservations)  // ✅ CRITICAL: Load reservations for seat count
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null)
            {
                TempData["ErrorMessage"] = "❌ Cinema not found.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Pass admin status to view
            ViewBag.IsAdmin = HttpContext.Session.GetString("UserRole") == "Admin";

            return View("Cinema-Details", cinema);
        }

        // GET: /Cinema/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                TempData["ErrorMessage"] = "❌ Cinema not found.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Calculate deletion impact
            var screeningCount = await _context.Screenings
                .CountAsync(s => s.CinemaId == id);

            var reservationCount = await _context.Reservations
                .CountAsync(r => r.Screening!.CinemaId == id);

            ViewBag.ScreeningCount = screeningCount;
            ViewBag.ReservationCount = reservationCount;

            return View("Cinema-Delete", cinema);
        }

        // POST: /Cinema/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var cinema = await _context.Cinemas.FindAsync(id);
                if (cinema == null)
                {
                    TempData["ErrorMessage"] = "❌ Cinema not found.";
                    return RedirectToAction(nameof(Index));
                }

                var screeningCount = await _context.Screenings.CountAsync(s => s.CinemaId == id);
                var reservationCount = await _context.Reservations.CountAsync(r => r.Screening!.CinemaId == id);

                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Cinema '{cinema.Name}' deleted! " +
                    $"({screeningCount} screenings and {reservationCount} reservations were removed)";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error: {ex.Message}";
                return RedirectToAction("Delete", new { id });
            }
        }
    }
}
