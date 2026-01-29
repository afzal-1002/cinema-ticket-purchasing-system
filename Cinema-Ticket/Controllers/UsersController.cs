using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using CinemaTicket.Services;
using CinemaTicket.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public UsersController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // ✅ GET: /Users or /Users/Index
        public async Task<IActionResult> Index()
        {
            // ✅ Only admins can view user list
            var isAdminString = HttpContext.Session.GetString("IsAdmin");
            if (isAdminString != "True")
            {
                TempData["ErrorMessage"] = "❌ Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            // ✅ Get all users from database
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // ✅ GET: /Users/UserList (alias for Index)
        public async Task<IActionResult> UserList()
        {
            return await Index();
        }

        // GET: /Users/Login
        public IActionResult Login()
        {
            return View("User-Login");
        }

        // POST: /Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "❌ Username and password are required.";
                return View("User-Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "❌ Invalid username or password.";
                return View("User-Login");
            }

            // ✅ CLEAR old session first to prevent corruption
            HttpContext.Session.Clear();
            
            // ✅ Delete all old cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            
            // ✅ Set session variables
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

            Console.WriteLine($"DEBUG Login: Set session - UserId={user.Id}, Username={user.Username}, IsAdmin={user.IsAdmin}");

            TempData["SuccessMessage"] = $"✅ Welcome back, {user.Username}!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Users/Logout
        public IActionResult Logout()
        {
            // ✅ Clear session
            HttpContext.Session.Clear();
            
            // ✅ ADDED: Delete all cookies to prevent data protection key issues
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            
            TempData["SuccessMessage"] = "✅ You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Users/Create
        [HttpGet]
        public IActionResult Create()
        {
            // ✅ Only admins can create users
            var isAdminString = HttpContext.Session.GetString("IsAdmin");
            if (isAdminString != "True")
            {
                TempData["ErrorMessage"] = "❌ Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            return View("User-Create");
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            // ✅ Verify admin access
            var isAdminString = HttpContext.Session.GetString("IsAdmin");
            if (isAdminString != "True")
            {
                TempData["ErrorMessage"] = "❌ Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            // ✅ Remove validation for ID (auto-generated)
            ModelState.Remove("Id");

            if (!ModelState.IsValid)
            {
                return View("User-Create", user);
            }

            try
            {
                // ✅ Check if username already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == user.Username);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View("User-Create", user);
                }

                // ✅ Check if email already exists
                if (!string.IsNullOrEmpty(user.Email))
                {
                    var existingEmail = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == user.Email);

                    if (existingEmail != null)
                    {
                        ModelState.AddModelError("Email", "Email already registered.");
                        return View("User-Create", user);
                    }
                }

                // ✅ Set creation date
                user.CreatedAt = DateTime.Now;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                Console.WriteLine($"SUCCESS: User '{user.Username}' created successfully (ID={user.Id})");
                TempData["SuccessMessage"] = $"✅ User '{user.Username}' created successfully!";
                
                return RedirectToAction("Details", new { id = user.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR creating user: {ex.Message}\n{ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"Error creating user: {ex.Message}");
                return View("User-Create", user);
            }
        }

        // GET: /Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "❌ User ID not provided.";
                return RedirectToAction("Index");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ User not found.";
                return RedirectToAction("Index");
            }

            return View("User-Details", user);
        }

        // GET: /Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var isAdminString = HttpContext.Session.GetString("IsAdmin");
            if (isAdminString != "True")
            {
                TempData["ErrorMessage"] = "❌ Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "❌ User ID not provided.";
                return RedirectToAction("Index");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ User not found.";
                return RedirectToAction("Index");
            }

            // ✅ Store original RowVersion for concurrency check
            if (user.RowVersion != null)
            {
                ViewBag.OriginalRowVersion = Convert.ToBase64String(user.RowVersion);
            }

            return View("User-Edit", user);
        }

        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            var isAdminString = HttpContext.Session.GetString("IsAdmin");
            if (isAdminString != "True")
            {
                TempData["ErrorMessage"] = "❌ Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            if (id != user.Id)
            {
                return BadRequest();
            }

            ModelState.Remove("Id");
            ModelState.Remove("CreatedAt"); // Remove validation for CreatedAt
            ModelState.Remove("RowVersion"); // Remove validation for RowVersion

            if (!ModelState.IsValid)
            {
                return View("User-Edit", user);
            }

            try
            {
                // ✅ Load existing user from database
                var existingUser = await _context.Users.FindAsync(id);

                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "❌ User not found.";
                    return RedirectToAction("Index");
                }

                // ✅ Update only the editable fields
                existingUser.Username = user.Username;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.IsAdmin = user.IsAdmin;

                // ✅ Only update password if it was changed (not empty)
                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    existingUser.Password = user.Password;
                }

                // ✅ Set original RowVersion for concurrency check
                if (user.RowVersion != null)
                {
                    _context.Entry(existingUser).OriginalValues["RowVersion"] = user.RowVersion;
                }

                // ✅ Update and save
                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                Console.WriteLine($"SUCCESS: User '{existingUser.Username}' (ID={id}) updated successfully");
                TempData["SuccessMessage"] = $"✅ User '{existingUser.Username}' updated successfully!";
                
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"CONCURRENCY ERROR updating user: {ex.Message}");
                
                // ✅ Check if user still exists
                if (!await _context.Users.AnyAsync(u => u.Id == id))
                {
                    TempData["ErrorMessage"] = "❌ User was deleted by another user. Changes cannot be saved.";
                    return RedirectToAction("Index");
                }

                // ✅ Show conflict resolution page
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();
                
                if (databaseValues == null)
                {
                    TempData["ErrorMessage"] = "❌ User was deleted by another user.";
                    return RedirectToAction("Index");
                }

                // ✅ Redirect to conflict page with user's attempted changes
                Console.WriteLine("Redirecting to User-Edit-Conflict page");
                return View("User-Edit-Conflict", user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR updating user: {ex.Message}\n{ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"❌ Error updating user: {ex.Message}");
                return View("User-Edit", user);
            }
        }

        // POST: /Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var isAdminString = HttpContext.Session.GetString("IsAdmin");
            if (isAdminString != "True")
            {
                TempData["ErrorMessage"] = "❌ Access denied. Admin only.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "❌ User not found.";
                return RedirectToAction("Index");
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ User '{user.Username}' deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR deleting user: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"❌ Error deleting user: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
