using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;
        private readonly IMongoCollection<Ticket> _tickets;

        public EmployeeRepository(IMongoDatabase database)
        {
            _employees = database.GetCollection<Employee>("Employee");
            _tickets = database.GetCollection<Ticket>("Ticket");
        }
        public async Task<List<Employee>> GetAllEmployees()
        {
            return await _employees.Find(FilterDefinition<Employee>.Empty).ToListAsync();
        }

        public async Task<List<Employee>> GetEmployeesWithTicket()
        {
            var pipeline = new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Ticket" },
                    { "localField", "_id" },
                    { "foreignField", "ReportedBy" },
                    { "as", "ReportedTickets" }
                }),
            };

            return await _employees.Aggregate<Employee>(pipeline).ToListAsync();
        }


        public async Task<Employee> GetEmployeeById(string? id)
        {
            return await _employees.Find(emp => emp.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Employee> GetEmployeeByEmail(string email)
        {
            var filter = Builders<Employee>.Filter.Regex(
                e => e.ContactInfo.Email,
                new BsonRegularExpression($"^{Regex.Escape(email)}$", "i")
            );

            return await _employees.Find(filter).FirstOrDefaultAsync();
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
