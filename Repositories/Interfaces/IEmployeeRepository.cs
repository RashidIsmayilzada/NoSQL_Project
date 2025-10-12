using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllEmployees();
        Task<Employee> GetEmployeeById(string? id);
        Task CreateEmployee(Employee employee);
        Task<bool> UpdateEmployee(Employee employee);   // ← تغییر
        Task DeleteEmployee(string? id);
        Task<List<Employee>> GetEmployeesWithTicket();
    }

}
