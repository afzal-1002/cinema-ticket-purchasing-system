using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using CinemaTicket.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Controllers
{
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICinemaService _cinemaService;

        public CinemasController(
            ApplicationDbContext context,
            ICinemaService cinemaService)
        {
            _context = context;
            _cinemaService = cinemaService;
        }

        // GET: /Cinemas
        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemaService.GetAllCinemasAsync();
            return View("~/Views/Cinema/Cinema-Index.cshtml", cinemas);
        }

        // GET: /Cinemas/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Cinema/Cinema-Create.cshtml");
        }

        // POST: /Cinemas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cinema cinema)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Cinema/Cinema-Create.cshtml", cinema);
            }

            bool success = await _cinemaService.CreateCinemaAsync(cinema);

            if (success)
            {
                TempData["Success"] = $"Cinema '{cinema.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to create cinema. Please try again.";
            return View("~/Views/Cinema/Cinema-Create.cshtml", cinema);
        }

        // GET: /Cinemas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();

            var cinema = await _cinemaService.GetCinemaByIdAsync(id.Value);

            if (cinema == null)
            {
                TempData["Error"] = "Cinema not found.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ ADDED: Store RowVersion in ViewBag for hidden field
            ViewBag.RowVersion = Convert.ToBase64String(cinema.RowVersion);

            return View("~/Views/Cinema/Cinema-Edit.cshtml", cinema);
        }

        // POST: /Cinemas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cinema cinema, string rowVersionBase64)
        {
            if (id != cinema.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                // ✅ ADDED: Restore RowVersion in ViewBag if returning to view
                ViewBag.RowVersion = rowVersionBase64;
                return View("~/Views/Cinema/Cinema-Edit.cshtml", cinema);
            }

            try
            {
                // ✅ FIXED: Convert Base64 string back to byte array
                byte[] originalRowVersion = Convert.FromBase64String(rowVersionBase64);

                // ✅ FIXED: Pass both cinema and originalRowVersion
                bool success = await _cinemaService.UpdateCinemaAsync(cinema, originalRowVersion);

                if (success)
                {
                    TempData["Success"] = $"Cinema '{cinema.Name}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to update cinema. Please try again.";
                ViewBag.RowVersion = rowVersionBase64;
                return View("~/Views/Cinema/Cinema-Edit.cshtml", cinema);
            }
            catch (DbUpdateConcurrencyException)
            {
                // ✅ ADDED: Handle concurrency conflicts
                TempData["Error"] = "The cinema was modified by another user. Please reload and try again.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                ViewBag.RowVersion = rowVersionBase64;
                return View("~/Views/Cinema/Cinema-Edit.cshtml", cinema);
            }
        }

        // GET: /Cinemas/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest();

            var cinema = await _cinemaService.GetCinemaByIdAsync(id.Value);

            if (cinema == null)
            {
                TempData["Error"] = "Cinema not found.";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Cinema/Cinema-Details.cshtml", cinema);
        }

        // GET: /Cinemas/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            var cinema = await _context.Cinemas
                .Include(c => c.Screenings!)
                    .ThenInclude(s => s.Reservations)
                .FirstOrDefaultAsync(c => c.Id == id.Value);

            if (cinema == null)
            {
                TempData["Error"] = "Cinema not found.";
                return RedirectToAction(nameof(Index));
            }

            // Pass screening and reservation counts to view
            ViewBag.ScreeningCount = cinema.Screenings?.Count ?? 0;
            ViewBag.ReservationCount = cinema.Screenings?
                .SelectMany(s => s.Reservations ?? new List<Reservation>())
                .Count() ?? 0;

            return View("~/Views/Cinema/Cinema-Delete.cshtml", cinema);
        }

        // POST: /Cinemas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cinema = await _cinemaService.GetCinemaByIdAsync(id);

            if (cinema == null)
            {
                TempData["Error"] = "Cinema not found.";
                return RedirectToAction(nameof(Index));
            }

            bool success = await _cinemaService.DeleteCinemaAsync(id);

            if (success)
            {
                TempData["Success"] = $"Cinema '{cinema.Name}' deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete cinema. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}