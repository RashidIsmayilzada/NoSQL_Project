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

        public EmployeeRepository(IMongoDatabase database)
        {
            _employees = database.GetCollection<Employee>("Employee");
        }

        // --- CRUD Operations ---
        public async Task<List<Employee>> GetAllEmployees()
        {
            return await _employees.Find(FilterDefinition<Employee>.Empty).ToListAsync();
        }

        public async Task<List<Employee>> GetAllEmployeesWithTickets()
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

        public async Task<Employee> GetEmployeeWithTicketsById(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var pipeline = new[]
            {
                    new BsonDocument("$match", new BsonDocument("_id", new ObjectId(id))),
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        { "from", "Ticket" },
                        { "localField", "_id" },
                        { "foreignField", "ReportedBy" },
                        { "as", "ReportedTickets" }
                    })
                };

            var results = await _employees.Aggregate<Employee>(pipeline).FirstOrDefaultAsync();
            return results;
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

        // --- Profile Management ---
        public async Task<bool> UpdateEmployeeProfile(Employee updatedEmployee)
        {
            if (string.IsNullOrEmpty(updatedEmployee.Id))
                throw new ArgumentException("Employee Id cannot be null or empty.", nameof(updatedEmployee.Id));

            var filter = Builders<Employee>.Filter.Eq(e => e.Id, updatedEmployee.Id);

            var update = Builders<Employee>.Update
                .Set(e => e.IsDisabled, updatedEmployee.IsDisabled)
                .Set(e => e.Name, updatedEmployee.Name)
                .Set(e => e.Role, updatedEmployee.Role)
                .Set(e => e.ContactInfo, updatedEmployee.ContactInfo);

            var result = await _employees.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        // --- Password Management ---
        public async Task<bool> UpdatePassword(string id, string hash)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Id, id);

            var update = Builders<Employee>.Update
                .Set(e => e.PasswordHashed, hash);

            var result = await _employees.UpdateOneAsync(filter, update);
            return result.ModifiedCount == 1;
        }

    }
}
