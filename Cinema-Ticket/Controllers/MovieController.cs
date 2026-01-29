using System.Threading.Tasks;
using CinemaTicket.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicket.Controllers
{
    public class MovieController : Controller
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // GET: /Movie
        public async Task<IActionResult> Index()
        {
            var titles = await _movieService.GetAllMovieTitlesAsync();
            return View("Movie-Index", titles);
        }

        // GET: /Movie/Details?title=Avengers
        public async Task<IActionResult> Details(string? title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest("Movie title is required.");
            }

            var screenings = await _movieService.GetScreeningsByTitleAsync(title);
            ViewBag.MovieTitle = title;
            return View("Movie-Details", screenings);
        }

        // GET: /Movie/Screening/5
        public async Task<IActionResult> Screening(int id)
        {
            var screening = await _movieService.GetScreeningByIdAsync(id);
            if (screening == null)
            {
                return NotFound();
            }

            return View("Movie-Screening", screening);
        }
    }
}
