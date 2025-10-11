using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    
    public class EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        : BaseLoggedInController
    {
        private readonly IEmployeeService _employeeService = employeeService;
        private readonly ILogger<EmployeeController> _logger = logger;

        // GET: /Employee
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Retrieving all employees");
                var employees = await _employeeService.GetAllEmployeesAsync();
                return View(employees);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while retrieving employees");
                TempData["Error"] = "Unable to retrieve employees. Please try again later.";
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving employees");
                TempData["Error"] = "An unexpected error occurred.";
                return View("Error");
            }
        }

        // GET: /Employee/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("Details called with null or empty ID");
                    TempData["Error"] = "Employee ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("Invalid employee ID format: {EmployeeId}", id);
                    TempData["Error"] = "Invalid employee ID format.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Retrieving employee with ID: {EmployeeId}", id);
                var employee = await _employeeService.GetEmployeeByIdAsync(id);

                if (employee == null)
                {
                    _logger.LogWarning("Employee not found with ID: {EmployeeId}", id);
                    TempData["Error"] = $"Employee with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(employee);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while retrieving employee {EmployeeId}", id);
                TempData["Error"] = "Unable to retrieve employee. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving employee {EmployeeId}", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Employee/EmployeesWithTickets
        [HttpGet]
        public async Task<IActionResult> EmployeesWithTickets()
        {
            try
            {
                _logger.LogInformation("Retrieving employees with their tickets");
                var employees = await _employeeService.GetEmployeesWithTicketAsync();
                return View(employees);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while retrieving employees with tickets");
                TempData["Error"] = "Unable to retrieve employees with tickets. Please try again later.";
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving employees with tickets");
                TempData["Error"] = "An unexpected error occurred.";
                return View("Error");
            }
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
            try
            {
                if (employee == null)
                {
                    _logger.LogWarning("Create called with null employee");
                    ModelState.AddModelError("", "Employee data is required.");
                    return View(employee);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for employee creation");
                    return View(employee);
                }

                _logger.LogInformation("Creating new employee: {EmployeeId}", employee.EmployeeId);
                await _employeeService.CreateEmployeeAsync(employee);

                TempData["Success"] = "Employee created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while creating employee");
                ModelState.AddModelError("", "Unable to create employee. Please try again later.");
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating employee");
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(employee);
            }
        }

        // GET: /Employee/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("Edit called with null or empty ID");
                    TempData["Error"] = "Employee ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("Invalid employee ID format: {EmployeeId}", id);
                    TempData["Error"] = "Invalid employee ID format.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Loading employee for edit: {EmployeeId}", id);
                var employee = await _employeeService.GetEmployeeByIdAsync(id);

                if (employee == null)
                {
                    _logger.LogWarning("Employee not found for edit with ID: {EmployeeId}", id);
                    TempData["Error"] = $"Employee with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(employee);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while loading employee for edit {EmployeeId}", id);
                TempData["Error"] = "Unable to load employee. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while loading employee for edit {EmployeeId}", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Employee/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee employee)
        {
            try
            {
                if (employee == null)
                {
                    _logger.LogWarning("Edit POST called with null employee");
                    TempData["Error"] = "Employee data is required.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrEmpty(employee.Id))
                {
                    _logger.LogWarning("Edit POST called with null or empty employee ID");
                    ModelState.AddModelError("", "Employee ID is required.");
                    return View(employee);
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(employee.Id, out _))
                {
                    _logger.LogWarning("Invalid employee ID format in edit: {EmployeeId}", employee.Id);
                    ModelState.AddModelError("", "Invalid employee ID format.");
                    return View(employee);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for employee edit: {EmployeeId}", employee.Id);
                    return View(employee);
                }

                _logger.LogInformation("Updating employee: {EmployeeId}", employee.Id);
                await _employeeService.UpdateEmployeeAsync(employee);

                TempData["Success"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while updating employee {EmployeeId}", employee?.Id);
                ModelState.AddModelError("", "Unable to update employee. Please try again later.");
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating employee {EmployeeId}", employee?.Id);
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(employee);
            }
        }

        // GET: /Employee/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("Delete called with null or empty ID");
                    TempData["Error"] = "Employee ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("Invalid employee ID format: {EmployeeId}", id);
                    TempData["Error"] = "Invalid employee ID format.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Loading employee for delete confirmation: {EmployeeId}", id);
                var employee = await _employeeService.GetEmployeeByIdAsync(id);

                if (employee == null)
                {
                    _logger.LogWarning("Employee not found for delete with ID: {EmployeeId}", id);
                    TempData["Error"] = $"Employee with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(employee);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while loading employee for delete {EmployeeId}", id);
                TempData["Error"] = "Unable to load employee. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while loading employee for delete {EmployeeId}", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Employee/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("DeleteConfirmed called with null or empty ID");
                    TempData["Error"] = "Employee ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("Invalid employee ID format in delete: {EmployeeId}", id);
                    TempData["Error"] = "Invalid employee ID format.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Deleting employee: {EmployeeId}", id);
                await _employeeService.DeleteEmployeeAsync(id);

                TempData["Success"] = "Employee deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while deleting employee {EmployeeId}", id);
                TempData["Error"] = "Unable to delete employee. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting employee {EmployeeId}", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
