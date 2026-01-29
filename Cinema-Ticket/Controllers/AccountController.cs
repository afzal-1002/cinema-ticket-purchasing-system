using CinemaTicket.Models.Entities;
using CinemaTicket.Services;
using CinemaTicket.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicket.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            // ✅ Use existing User-Login view from Users folder
            return View("~/Views/Users/User-Login.cshtml");
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Username and password are required.";
                return View("~/Views/Users/User-Login.cshtml");
            }

            var user = await _userService.AuthenticateAsync(username, password);

            if (user == null)
            {
                TempData["Error"] = "Invalid username or password.";
                return View("~/Views/Users/User-Login.cshtml");
            }

            // Store session data using SessionHelper
            HttpContext.Session.SetUserId(user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

            TempData["Success"] = $"Welcome back, {user.FirstName ?? user.Username}!";

            // Redirect to return URL or appropriate dashboard
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.IsAdmin)
            {
                return RedirectToAction("Index", "Screenings");
            }

            return RedirectToAction("Index", "Screenings");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // ✅ Use existing User-Register view if it exists
            if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Views/Users/User-Register.cshtml")))
            {
                return View("~/Views/Users/User-Register.cshtml");
            }
            
            // Fallback to default location
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(user.Username) || 
                string.IsNullOrWhiteSpace(user.Password) ||
                string.IsNullOrWhiteSpace(user.Email))
            {
                TempData["Error"] = "Username, password, and email are required.";
                return View("~/Views/Users/User-Register.cshtml", user);
            }

            if (user.Password != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return View("~/Views/Users/User-Register.cshtml", user);
            }

            // Check if username already exists
            var existingUser = await _userService.GetUserByUsernameAsync(user.Username);
            if (existingUser != null)
            {
                TempData["Error"] = "Username already exists.";
                return View("~/Views/Users/User-Register.cshtml", user);
            }

            // Set default values
            user.IsAdmin = false;
            
            bool success = await _userService.CreateUserAsync(user);

            if (success)
            {
                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Registration failed. Please try again.";
            return View("~/Views/Users/User-Register.cshtml", user);
        }
    }
}