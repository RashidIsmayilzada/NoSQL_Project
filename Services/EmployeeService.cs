using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        public EmployeeService(IEmployeeRepository employeeRepository) {
            _employeeRepository = employeeRepository;
        }
        
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetAllEmployees();
        }
        
        public async Task<Employee> GetEmployeeByIdAsync(string? id)
        {
            return await _employeeRepository.GetEmployeeById(id);
        }
        
        public async Task CreateEmployeeAsync(Employee employee)
        {
            await _employeeRepository.CreateEmployee(employee);
        }
        
        public async Task UpdateEmployeeAsync(Employee employee)
        {
            await _employeeRepository.UpdateEmployee(employee);
        }

        public async Task DeleteEmployeeAsync(string? id)
        {
            await _employeeRepository.DeleteEmployee(id);
        }
    }
}
