using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee> GetEmployeeByIdAsync(string? id);
        Task CreateEmployeeAsync(Employee employee);
        Task<bool> UpdateEmployeeAsync(Employee employee);   // ← تغییر
        Task DeleteEmployeeAsync(string? id);
        Task<List<Employee>> GetEmployeesWithTicketAsync();
    }



}
