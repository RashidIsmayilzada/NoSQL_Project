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

            Employee employee = await _employeeService.GetEmployeeByIdAsync(id);


            return View(employee);
        }
        
        // GET: /Employee/EmployeesWithTickets
        [HttpGet]
        public async Task<IActionResult> EmployeesWithTickets()
        {
            List<Employee> employees = await _employeeService.GetEmployeesWithTicketAsync();
            return Ok(employees);
        }

        // GET: /Employee/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Employee/Create
        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _employeeService.CreateEmployeeAsync(employee);
            return View(new { message = "Employee created successfully", id = employee.Id });
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
        public async Task<IActionResult> Update(Employee employee)
        {
            if (employee == null)
            {
                return BadRequest("Employee is null");
            }

            if (string.IsNullOrEmpty(employee.Id))
            {
                return BadRequest("Employee Id is missing");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _employeeService.UpdateEmployeeAsync(employee);
            return View(new { message = "Employee updated successfully", id = employee.Id, employee = employee });
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
