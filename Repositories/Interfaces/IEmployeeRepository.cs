using NoSQL_Project.Models;
using NoSQL_Project.ViewModels;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        // --- CRUD Operations ---
        Task<List<Employee>> GetAllEmployees();
        Task<List<Employee>> GetAllEmployeesWithTickets();
        Task<Employee> GetEmployeeById(string? id);
        Task<Employee> GetEmployeeWithTicketsById(string? id);
        Task<Employee> GetEmployeeByEmail(string email);
        Task CreateEmployee(Employee employee);
        Task UpdateEmployee(Employee employee);
        Task DeleteEmployee(string? id);

        // --- Profile Management ---
        Task<bool> UpdateEmployeeProfile(Employee updatedEmployee);

        // --- Password Management ---
        Task<bool> UpdatePassword(string id, string hash);

    }
}
