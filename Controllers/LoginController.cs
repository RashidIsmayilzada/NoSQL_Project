using Microsoft.AspNetCore.Mvc;

namespace NoSQL_Project.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
