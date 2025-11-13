using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.ViewModels.Employee;
using NoSQL_Project.Utilities;

namespace NoSQL_Project.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // -------------------------------
        // READ: Get all employees with ticket counts
        // -------------------------------
        public async Task<IReadOnlyList<EmployeeListViewModel>> GetListAsync()
        {
            var employees = await _employeeRepository.GetAllEmployeesWithTickets();

            return employees.Select(e => new EmployeeListViewModel
            {
                Id = e.Id ?? "",
                IsDisabled = e.IsDisabled,
                Name = e.Name,
                Role = e.Role,
                Email = e.ContactInfo.Email,
                ReportedTicketCount = e.ReportedTickets?.Count ?? 0
            }).ToList();
        }

        // -------------------------------
        // READ: Single employee by ID
        // -------------------------------
        public async Task<EmployeeViewModel?> GetEmployeeAsync(string id)
        {
            var employee = await _employeeRepository.GetEmployeeById(id);
            if (employee == null) return null;

            return new EmployeeViewModel
            {
                Id = employee.Id ?? "",
                IsDisabled = employee.IsDisabled,
                Name = employee.Name,
                Role = employee.Role,
            };
        }

        // -------------------------------
        // READ: Single employee with tickets by ID
        // -------------------------------
        public async Task<EmployeeDetailsViewModel?> GetDetailsAsync(string id)
        {
            var employee = await _employeeRepository.GetEmployeeWithTicketsById(id);
            if (employee == null) return null;

            return new EmployeeDetailsViewModel
            {
                Id = employee.Id ?? "",
                IsDisabled = employee.IsDisabled,
                Name = employee.Name,
                Role = employee.Role,
                ContactInfo = employee.ContactInfo,
                ReportedTickets = employee.ReportedTickets ?? new List<Ticket>()
            };
        }

        // -------------------------------
        // READ: Employees with tickets
        // -------------------------------
        public async Task<IReadOnlyList<EmployeeDetailsViewModel>> GetWithTicketsAsync()
        {
            var employees = await _employeeRepository.GetAllEmployeesWithTickets();

            return employees.Select(e => new EmployeeDetailsViewModel
            {
                Id = e.Id ?? "",
                IsDisabled = e.IsDisabled,
                Name = e.Name,
                Role = e.Role,
                ContactInfo = e.ContactInfo,
                ReportedTickets = e.ReportedTickets ?? new List<Ticket>()
            }).ToList();
        }

        // -------------------------------
        // AUTHENTICATE
        // -------------------------------
        public async Task<EmployeeDetailsViewModel?> AuthenticateAsync(LoginViewModel vm)
        {
            var employee = await _employeeRepository.GetEmployeeByEmail(vm.Email);
            if (employee == null) return null;
            if (employee.IsDisabled) return null;

            if (!PasswordHelper.VerifyPassword(vm.Password, employee.PasswordHashed))
                return null;

            return new EmployeeDetailsViewModel
            {
                Id = employee.Id ?? "",
                IsDisabled = employee.IsDisabled,
                Name = employee.Name,
                Role = employee.Role,
                ContactInfo = employee.ContactInfo,
                ReportedTickets = employee.ReportedTickets ?? new List<Ticket>()
            };
        }

        // -------------------------------
        // CREATE
        // -------------------------------
        public async Task<string> CreateAsync(EmployeeCreateViewModel vm)
        {
            string hash = PasswordHelper.HashPassword(vm.Password);

            Employee employee = new Employee
            {
                IsDisabled = vm.IsDisabled,
                Name = vm.Name,
                Role = vm.Role,
                ContactInfo = vm.ContactInfo,
                PasswordHashed = hash,
                ReportedTickets = new List<Ticket>()
            };

            await _employeeRepository.CreateEmployee(employee);
            return employee.Id ?? "";
        }

        // -------------------------------
        // UPDATE PROFILE (non-password)
        // -------------------------------
        public async Task<bool> UpdateProfileAsync(EmployeeDetailsViewModel vm)
        {
            var employee = await _employeeRepository.GetEmployeeById(vm.Id);
            if (employee == null) return false;

            employee.IsDisabled = vm.IsDisabled;
            employee.Name = vm.Name;
            employee.Role = vm.Role;
            employee.ContactInfo = vm.ContactInfo;

            return await _employeeRepository.UpdateEmployeeProfile(employee);
        }

        // -------------------------------
        // CHANGE PASSWORD
        // -------------------------------
        public async Task<bool> ChangePasswordAsync(PasswordChangeViewModel vm)
        {
            if (vm.NewPassword != vm.ConfirmNewPassword)
                return false;

            var employee = await _employeeRepository.GetEmployeeById(vm.Id);
            if (employee == null) return false;

            string PasswordHashed = PasswordHelper.HashPassword(vm.NewPassword);

            return await _employeeRepository.UpdatePassword(vm.Id, PasswordHashed);
        }

        // -------------------------------
        // DELETE
        // -------------------------------
        public async Task<bool> DeleteAsync(string id)
        {

            //------------------------------------------------------------------------------------!!!!!!
            //TO DO - Check if employee has any tickets related to them and block deletion if so!!!!!!!!
            //------------------------------------------------------------------------------------!!!!!!

            await _employeeRepository.DeleteEmployee(id);
            return true;
        }

    }
}
