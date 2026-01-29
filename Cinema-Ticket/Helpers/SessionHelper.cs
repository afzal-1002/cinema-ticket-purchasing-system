using Microsoft.AspNetCore.Http;

namespace CinemaTicket.Helpers
{
    public static class SessionHelper
    {
        // ✅ CHANGED: Return non-nullable int, throw if not logged in
        public static int GetUserId(this ISession session)
        {
            var userId = session.GetInt32("UserId");
            
            if (userId == null)
            {
                throw new InvalidOperationException("User is not logged in. UserId not found in session.");
            }
            
            return userId.Value;
        }

        public static string GetUsername(this ISession session)
        {
            return session.GetString("Username") ?? "Guest";
        }

        public static bool IsUserLoggedIn(this ISession session)
        {
            return session.GetInt32("UserId").HasValue;
        }

        public static bool IsAdmin(this ISession session)
        {
            var isAdminStr = session.GetString("IsAdmin");
            return bool.TryParse(isAdminStr, out bool isAdmin) && isAdmin;
        }

        public static void SetUserId(this ISession session, int userId)
        {
            session.SetInt32("UserId", userId);
        }

        public static void ClearSession(this ISession session)
        {
            session.Clear();
        }
    }
}