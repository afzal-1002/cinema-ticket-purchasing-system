using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Controllers
{
    public class MoviesController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Movies/Details?title=Inception
        public IActionResult Details(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return BadRequest();

            var screenings = _context.Screenings
                .Where(s => s.MovieTitle == title && s.StartTime > System.DateTime.Now)
                .OrderBy(s => s.StartTime)
                .Include(s => s.Cinema)
                .ToList();

            if (!screenings.Any()) return NotFound();

            ViewBag.MovieTitle = title;
            return View(screenings);
        }
    }
}
