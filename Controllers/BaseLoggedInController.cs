using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NoSQL_Project.Extensions;
using NoSQL_Project.Models;

namespace NoSQL_Project.Controllers
{
    //Used to check if employee is logged in and to send employee data to view
    public abstract class BaseLoggedInController : Controller
    {
        public Employee? LoggedInEmployee;
        public BaseLoggedInController()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInEmployee");
                ViewData["LoggedInEmployee"] = LoggedInEmployee;

                // Check if user is logged in and if not send them to login page
                if (LoggedInEmployee == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to be able to access that page";
                    context.Result = new RedirectToActionResult("Login", "Login", null);
                    ViewBag.ErrorMessage = "You need to be logged in to be able to access that page";
                    return;
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "A session error occurred. Please log in again.";
                context.Result = new RedirectToActionResult("Login", "Login", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
