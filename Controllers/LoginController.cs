using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Extensions;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    public class LoginController : Controller
    {
        private readonly IEmployeeService _employeesService;
        public LoginController(IEmployeeService employeesService)
        {
            _employeesService = employeesService;
        }
        public IActionResult Index()
        {
            Employee? loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInEmployee");
            if (loggedInEmployee == null)
                return RedirectToAction("Login", "Login");

            ViewData["LoggedInEmployee"] = loggedInEmployee;

            return RedirectToAction("Index", "Employee");
        }

        public IActionResult Login()
        {
            Employee? loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInEmployee");
            if (loggedInEmployee != null)
                return RedirectToAction("Index", "Employee");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginCredentials loginModel)
        {
            Employee? employee = await _employeesService.GetEmployeeByLoginCredentialsAsync(loginModel.Email, loginModel.Password);

            if (employee is null)
            {
                TempData["ErrorMessage"] = "Bad number/password combination";

                return View(loginModel);
            }
            else
            {
                HttpContext.Session.SetObject("LoggedInEmployee", employee);

                switch (employee.Role)
                {
                    case RoleType.ServiceDesk:
                        return RedirectToAction("Index", "Home");
                    case RoleType.Regular:
                        return RedirectToAction("Index", "Employee");
                    default: return NotFound();
                }
            }
        }


        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Login");
        }
    }
}
