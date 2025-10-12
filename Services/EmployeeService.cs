using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public Task<List<Employee>> GetAllEmployeesAsync()
        => _employeeRepository.GetAllEmployees();

    public Task<Employee> GetEmployeeByIdAsync(string? id)
        => _employeeRepository.GetEmployeeById(id);

    public Task CreateEmployeeAsync(Employee employee)
        => _employeeRepository.CreateEmployee(employee);

    public Task<bool> UpdateEmployeeAsync(Employee employee)          // ← تغییر
        => _employeeRepository.UpdateEmployee(employee);

    public Task DeleteEmployeeAsync(string? id)
        => _employeeRepository.DeleteEmployee(id);

    public Task<List<Employee>> GetEmployeesWithTicketAsync()
        => _employeeRepository.GetEmployeesWithTicket();
}
