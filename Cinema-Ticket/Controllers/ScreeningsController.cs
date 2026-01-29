using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using CinemaTicket.Services;
using CinemaTicket.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Controllers
{
    public class ScreeningsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IScreeningService _screeningService;

        public ScreeningsController(
            ApplicationDbContext context,
            IScreeningService screeningService)
        {
            _context = context;
            _screeningService = screeningService;
        }

        // GET: /Screenings or /Screenings/Index
        public async Task<IActionResult> Index()
        {
            // ✅ Get all screenings with Cinema data included
            var screenings = await _context.Screenings
                .Include(s => s.Cinema)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return View(screenings);
        }

        // GET: /Screenings/Create (maps to Screening-Create.cshtml)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Cinemas = await _context.Cinemas.ToListAsync();
            return View("Screening-Create");  // ✅ Explicit view name
        }

        // POST: /Screenings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Screening screening)
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cinemas = await _context.Cinemas.ToListAsync();
                return View("Screening-Create", screening);  // ✅ Explicit view name
            }

            bool success = await _screeningService.CreateScreeningAsync(screening);

            if (success)
            {
                TempData["Success"] = $"Screening for '{screening.MovieTitle}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to create screening. Please try again.";
            ViewBag.Cinemas = await _context.Cinemas.ToListAsync();
            return View("Screening-Create", screening);  // ✅ Explicit view name
        }

        // GET: /Screenings/Edit/5 (maps to Screening-Edit.cshtml)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return BadRequest();

            var screening = await _screeningService.GetScreeningByIdAsync(id.Value);

            if (screening == null)
            {
                TempData["Error"] = "Screening not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Cinemas = await _context.Cinemas.ToListAsync();
            ViewBag.RowVersion = Convert.ToBase64String(screening.RowVersion);

            return View("Screening-Edit", screening);  // ✅ Explicit view name
        }

        // POST: /Screenings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Screening screening, string rowVersionBase64)
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            if (id != screening.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Cinemas = await _context.Cinemas.ToListAsync();
                ViewBag.RowVersion = rowVersionBase64;
                return View("Screening-Edit", screening);  // ✅ Explicit view name
            }

            try
            {
                byte[] originalRowVersion = Convert.FromBase64String(rowVersionBase64);
                bool success = await _screeningService.UpdateScreeningAsync(screening, originalRowVersion);

                if (success)
                {
                    TempData["Success"] = $"Screening for '{screening.MovieTitle}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to update screening. Please try again.";
                ViewBag.Cinemas = await _context.Cinemas.ToListAsync();
                ViewBag.RowVersion = rowVersionBase64;
                return View("Screening-Edit", screening);  // ✅ Explicit view name
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "The screening was modified by another user. Please reload and try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Screenings/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            var screening = await _screeningService.GetScreeningByIdAsync(id);

            if (screening == null)
            {
                TempData["Error"] = "Screening not found.";
                return RedirectToAction(nameof(Index));
            }

            bool success = await _screeningService.DeleteScreeningAsync(id);

            if (success)
            {
                TempData["Success"] = $"Screening for '{screening.MovieTitle}' deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete screening. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Screenings/Details/5 (maps to Screening-Details.cshtml)
        public async Task<IActionResult> Details(int id)
        {
            var screening = await _context.Screenings
                .Include(s => s.Cinema)
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null)
            {
                return NotFound();
            }

            return View("Screening-Details", screening);  // ✅ Explicit view name
        }
    }
}
