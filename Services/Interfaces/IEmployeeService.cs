using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<List<Employee>> GetEmployeesWithTicketAsync();
        Task<Employee> GetEmployeeByIdAsync(string? id);
        Task<Employee> GetEmployeeByLoginCredentialsAsync(string email, string password);
        Task CreateEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string? id);
        
         
    }
}
