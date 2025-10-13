using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllEmployees();
        Task<List<Employee>> GetEmployeesWithTicket();
        Task<Employee> GetEmployeeById(string? id);

        Task<Employee> GetEmployeeByLoginCredentials(string email, string passwordHashed);
        Task CreateEmployee(Employee employee);
        Task UpdateEmployee(Employee employee);
        Task DeleteEmployee(string? id);

    }
}
