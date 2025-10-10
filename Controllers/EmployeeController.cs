using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: /Employee
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return View(employees);
        }

        // GET: /Employee/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id);


            return View(employee);
        }

        // GET: /Employee/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                await _employeeService.CreateEmployeeAsync(employee);
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // GET: /Employee/Update/{id}
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            return View(employee);
        }

        // POST: /Employee/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Employee employee)
        {
            if (ModelState.IsValid)
            {
                await _employeeService.UpdateEmployeeAsync(employee);
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        // GET: /Employee/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            return View(employee);
        }

        // POST: /Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
