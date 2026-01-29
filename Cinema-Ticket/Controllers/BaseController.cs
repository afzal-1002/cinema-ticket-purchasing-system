using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace CinemaTicket.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Allow anonymous access to Login/Register actions in Users controller
            var routeValues = context.RouteData.Values;
            var controller = (routeValues.ContainsKey("controller") ? routeValues["controller"]?.ToString() : string.Empty)?.ToLower();
            var action = (routeValues.ContainsKey("action") ? routeValues["action"]?.ToString() : string.Empty)?.ToLower();

            if (controller == "users" && (action == "login" || action == "register"))
            {
                base.OnActionExecuting(context);
                return;
            }

            // Check if UserId exists in the session
            if (context.HttpContext.Session.GetInt32("UserId") == null)
            {
                // Redirect to the login page if the session is not set
                context.Result = new RedirectToActionResult("Login", "Users", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
