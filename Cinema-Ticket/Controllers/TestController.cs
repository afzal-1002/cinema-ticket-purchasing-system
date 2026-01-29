using CinemaTicket.Data;
using CinemaTicket.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaTicket.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Test/ConcurrencyDemo
        public async Task<IActionResult> ConcurrencyDemo()
        {
            try
            {
                // Load existing user from init.sql
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "jane");

                if (user == null)
                {
                    ViewBag.Error = "❌ Test user 'jane' not found. Run docker-compose up -d to initialize database.";
                    return View();
                }

                var userId = user.Id;
                var originalEmail = user.Email;
                var originalRowVersion = user.RowVersion;

                ViewBag.UserId = userId;
                ViewBag.OriginalEmail = originalEmail;
                ViewBag.User1OriginalTimestamp = Convert.ToBase64String(originalRowVersion);
                ViewBag.User2OriginalTimestamp = Convert.ToBase64String(originalRowVersion);

                // User1 update (succeeds)
                var user1 = await _context.Users.FindAsync(userId);
                if (user1 == null)
                {
                    ViewBag.Error = "❌ User not found during test.";
                    return View();
                }

                user1.Email = "jane_updated_" + DateTime.Now.Ticks + "@email.com";
                await _context.SaveChangesAsync();

                ViewBag.User1NewTimestamp = Convert.ToBase64String(user1.RowVersion);
                ViewBag.User1Success = "✅ User1 update succeeded";
                ViewBag.User1NewEmail = user1.Email;

                // User2 concurrent update (will fail - this is EXPECTED)
                try
                {
                    // Detach to simulate fresh context
                    _context.Entry(user1).State = EntityState.Detached;

                    // Create user with OLD RowVersion (simulating stale data)
                    var user2 = new User
                    {
                        Id = userId,
                        Username = user1.Username,
                        Password = user1.Password,
                        Email = "jane_user2_attempt@email.com",
                        FirstName = user1.FirstName,
                        LastName = user1.LastName,
                        PhoneNumber = user1.PhoneNumber,
                        IsAdmin = user1.IsAdmin,
                        RowVersion = originalRowVersion  // OLD timestamp - will cause conflict
                    };

                    _context.Users.Attach(user2);
                    _context.Entry(user2).State = EntityState.Modified;
                    await _context.SaveChangesAsync(); // This SHOULD throw DbUpdateConcurrencyException

                    ViewBag.User2Result = "❌ FAILED - concurrency check didn't work!";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // This is EXPECTED - it proves timestamp concurrency is working!
                    ViewBag.User2Result = $"✅ Concurrency conflict detected! {ex.Message}";
                    
                    // Clear the change tracker to prevent issues with restore
                    foreach (var entry in _context.ChangeTracker.Entries().ToList())
                    {
                        entry.State = EntityState.Detached;
                    }
                }

                // Show final state
                var final = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (final != null)
                {
                    ViewBag.FinalEmail = final.Email;
                    ViewBag.FinalTimestamp = Convert.ToBase64String(final.RowVersion);
                }

                // Restore original email for next test (WRAP IN TRY/CATCH)
                try
                {
                    var restore = await _context.Users.FindAsync(userId);
                    if (restore != null)
                    {
                        restore.Email = originalEmail;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If restoration fails due to concurrency, ignore it
                    ViewBag.RestoreWarning = "⚠️ Note: Could not restore original email due to concurrent modification.";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"❌ Unexpected error during test: {ex.Message}";
                return View();
            }
        }

        // GET: /Test/AdminConcurrencyTest
        public IActionResult AdminConcurrencyTest()
        {
            return View();
        }

        // GET: /Test/SessionDebug
        public IActionResult SessionDebug()
        {
            return View();
        }
    }
}