using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Models;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Models.DTOs;
using CinemaTicket.Services;
using Microsoft.AspNetCore.Http;
using CinemaTicket.Data;
using CinemaTicket.Helpers;

namespace CinemaTicket.Controllers;

public class HomeController : Controller
{
    private readonly CinemaTicket.Data.ApplicationDbContext _context;
    private readonly CinemaTicket.Services.IReservationService _reservationService;
    private readonly CinemaTicket.Services.IUserService _userService;
    private readonly CinemaTicket.Services.ICinemaService _cinemaService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        CinemaTicket.Data.ApplicationDbContext context,
        CinemaTicket.Services.IReservationService reservationService,
        CinemaTicket.Services.IUserService userService,
        CinemaTicket.Services.ICinemaService cinemaService,
        ILogger<HomeController> logger)
    {
        _context = context;
        _reservationService = reservationService;
        _userService = userService;
        _cinemaService = cinemaService;
        _logger = logger;
    }

    // DEBUG helper
    [HttpGet("/debug/login-as-admin")]
    public IActionResult LoginAsAdmin()
    {
        HttpContext.Session.SetInt32("UserId", 1);
        HttpContext.Session.SetString("IsAdmin", "true");
        return RedirectToAction("Welcome");
    }

    public IActionResult Welcome()
    {
        var sessionUser = HttpContext.Session.GetInt32("UserId");
        if (!sessionUser.HasValue)
            return RedirectToAction("Login", "Users");

        return View();
    }

    // GET: /Home/Dashboard - redirect to Index
    public IActionResult Dashboard()
    {
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index(string? q = null)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var isAdmin = HttpContext.Session.IsAdmin();

        // Get user's reservations if logged in
        if (userId.HasValue)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Screening)
                .Where(r => r.UserId == userId.Value)
                .OrderBy(r => r.Screening!.StartTime)
                .Select(r => new
                {
                    r.Id,
                    r.SeatNumber,
                    MovieTitle = r.Screening!.MovieTitle,
                    StartTime = r.Screening.StartTime
                })
                .ToListAsync();

            ViewBag.UserReservations = reservations;
        }

        // Get upcoming screenings (next 7 days)
        var now = DateTime.Now;
        var nextWeek = now.AddDays(7);

        var upcomingScreenings = await _context.Screenings
            .Include(s => s.Cinema)
            .Include(s => s.Reservations)
            .Where(s => s.StartTime >= now && s.StartTime <= nextWeek)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        // Filter by search query if provided
        if (!string.IsNullOrWhiteSpace(q))
        {
            upcomingScreenings = upcomingScreenings
                .Where(s => s.MovieTitle.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
            ViewBag.SearchQuery = q;
        }

        // Group by movie title
        var groupedMovies = upcomingScreenings
            .GroupBy(s => s.MovieTitle)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => new
                {
                    s.Id,
                    s.MovieTitle,
                    s.StartTime,
                    CinemaName = s.Cinema?.Name ?? "Unknown",
                    AvailableSeats = (s.Cinema?.TotalRows ?? 0) * (s.Cinema?.SeatsPerRow ?? 0) - (s.Reservations?.Count ?? 0)
                }).AsEnumerable()
            );

        ViewBag.UpcomingMovies = groupedMovies;

        // Create movie cards for quick view
        var movieCards = groupedMovies.Select(g => new
        {
            Title = g.Key,
            RemainingSeats = g.Value.Sum(s => s.AvailableSeats)
        }).ToList();

        ViewBag.UpcomingMovieCards = movieCards;

        return View("Dashboard");
    }

    public async Task<IActionResult> ScreeningDetails(int screeningId)
    {
        // ✅ ADD NULL CHECK
        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.Id == screeningId);

        if (screening?.Cinema == null)
        {
            // Handle missing screening/cinema
            TempData["ErrorMessage"] = "❌ Screening not found.";
            return RedirectToAction("Index");
        }

        int totalCapacity = screening.Cinema.TotalRows * screening.Cinema.SeatsPerRow;
        int reservedSeats = screening.Reservations?.Count ?? 0;
        int availableSeats = totalCapacity - reservedSeats;

        ViewBag.Screening = screening;
        ViewBag.AvailableSeats = availableSeats;

        return View();
    }

    // GET: /Home/Privacy
    public IActionResult Privacy()
    {
        // ✅ Privacy page only for logged-in users
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            TempData["ErrorMessage"] = "❌ Please log in to view privacy policy.";
            return RedirectToAction("Login", "Users");
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
