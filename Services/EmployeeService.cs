using System.Security.Cryptography;
using System.Text;
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
        
        public async Task<List<Employee>> GetEmployeesWithTicketAsync()
        {
            return await _employeeRepository.GetEmployeesWithTicket();
        }

        public async Task<Employee?> GetEmployeeByLoginCredentialsAsync(string email, string password)
        {
            var emp = await _employeeRepository.GetEmployeeByEmail(email);
            if (emp is null)
                return null;

            // Check if employee is disabled
            if (emp.IsDisabled)
                return null;

            // decode stored salt
            byte[] saltBytes = Convert.FromBase64String(emp.Salt);

            // re-hash entered password with stored salt
            string enteredHash = HashPassword(password, saltBytes);

            // compare safely
            return enteredHash == emp.Password ? emp : null;
        }

        // Hash password with SHA-256
        private string HashPassword(string password, byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Combine password + salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combined = new byte[passwordBytes.Length + salt.Length];

                Buffer.BlockCopy(passwordBytes, 0, combined, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, combined, passwordBytes.Length, salt.Length);

                byte[] hashBytes = sha256.ComputeHash(combined);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static byte[] GenerateSalt(int size = 16)
        {
            // size = number of bytes (16 = 128 bits, which is standard)
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
