using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IEmployeeService
    {
        List<Employee> GetAllEmployees();
    }
}
