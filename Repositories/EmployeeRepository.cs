using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;

        public EmployeeRepository(IMongoDatabase database)
        {
            _employees = database.GetCollection<Employee>("Employee");
        }
        public async Task<List<Employee>> GetAllEmployees()
        {
            return await _employees.Find(FilterDefinition<Employee>.Empty).ToListAsync();
        }
        
        public async Task<Employee> GetEmployeeById(string? id)
        {
            return await _employees.Find(emp => emp.Id == id).FirstOrDefaultAsync();
        }
        
        public async Task CreateEmployee(Employee employee)
        {
            await _employees.InsertOneAsync(employee);
        }
        
        public async Task UpdateEmployee(Employee employee)
        {
            await _employees.ReplaceOneAsync(emp => emp.Id == employee.Id, employee);
        }

        public async Task DeleteEmployee(string? id)
        {
            await _employees.DeleteOneAsync(emp => emp.Id == id);
        }
    }
}
