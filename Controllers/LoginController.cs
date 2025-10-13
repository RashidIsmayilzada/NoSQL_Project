using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginCredentials loginModel)
        {
            if (!ModelState.IsValid)
                return View(loginModel);

            // IMPORTANT: Prefer a service method that returns employee by email and a separate password verifier.
            Employee? employee = await _employeesService.GetEmployeeByLoginCredentialsAsync(loginModel.Email, loginModel.Password);
            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "Incorrect email or password.");
                return View(loginModel);
            }

            // create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{employee.Name.FirstName} {employee.Name.LastName}"),
                new Claim(ClaimTypes.Role, employee.Role.ToString()),
                new Claim("Email", employee.ContactInfo.Email ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, "MyCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookie", principal, new AuthenticationProperties
            {
                IsPersistent = true // or false depending on remember-me
            });

            // redirect based on role
            return employee.Role switch
            {
                RoleType.ServiceDesk => RedirectToAction("Index", "Home"),
                RoleType.Regular => RedirectToAction("Index", "Employee"),
                _ => RedirectToAction("Index", "Employee")
            };
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookie");
            return RedirectToAction("Login", "Login");
        }
    }
}
