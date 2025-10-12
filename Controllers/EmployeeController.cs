using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    
    public class EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        : Controller
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
            return View(new Employee
            {
                Name = new Name(),
                ContactInfo = new ContactInfo()
            });
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
                    ModelState.AddModelError("", "Employee data is required.");
                    return View(employee);
                }

                // اطمینان از نال نبودن ساب‌اُبجکت‌ها
                employee.Name ??= new Name();
                employee.ContactInfo ??= new ContactInfo();

                if (!ModelState.IsValid) return View(employee);

                // نرمال‌سازی ایمیل
                if (!string.IsNullOrWhiteSpace(employee.ContactInfo.Email))
                    employee.ContactInfo.Email = employee.ContactInfo.Email.Trim().ToLowerInvariant();

                await _employeeService.CreateEmployeeAsync(employee);
                TempData["Success"] = "Employee created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException)
            {
                ModelState.AddModelError("", "Unable to create employee. Please try again later.");
                return View(employee);
            }
            catch
            {
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
        public async Task<IActionResult> Edit(string id, Employee employee)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employee?.Id))
                    employee.Id = id;

                if (string.IsNullOrWhiteSpace(employee.Id) || !ObjectId.TryParse(employee.Id, out _))
                {
                    ModelState.AddModelError("", "Invalid employee ID.");
                    return View(employee);
                }

                employee.Name ??= new Name();
                employee.ContactInfo ??= new ContactInfo();
                ModelState.Remove("ReportedTickets"); // اگه در مدل هست

                if (!ModelState.IsValid) return View(employee);

                if (!string.IsNullOrWhiteSpace(employee.ContactInfo.Email))
                    employee.ContactInfo.Email = employee.ContactInfo.Email.Trim().ToLowerInvariant();

                var ok = await _employeeService.UpdateEmployeeAsync(employee);   // ← حالا bool
                if (!ok)
                {
                    ModelState.AddModelError("", "Update failed or employee not found.");
                    return View(employee);
                }

                TempData["Success"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "DB error while updating employee {Id}", employee?.Id);
                ModelState.AddModelError("", "Database error. Please try again later.");
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating employee {Id}", employee?.Id);
                ModelState.AddModelError("", "Unexpected error occurred.");
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
