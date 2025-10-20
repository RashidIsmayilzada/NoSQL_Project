using NoSQL_Project.ViewModels.Employee;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IEmployeeService
    {
        // READ
        Task<IReadOnlyList<EmployeeListViewModel>> GetListAsync();                 // for Index

        Task<EmployeeViewModel?> GetEmployeeAsync(string id);                 // for basic info
        Task<EmployeeDetailsViewModel?> GetDetailsAsync(string id);                // for Details/Edit
        Task<IReadOnlyList<EmployeeDetailsViewModel>> GetWithTicketsAsync();       // optional page: employees + their tickets

        // AUTH
        Task<EmployeeDetailsViewModel?> AuthenticateAsync(LoginViewModel vm);      // null = bad creds

        // CREATE (password is entered in the form; no auto-generation)
        Task<string> CreateAsync(EmployeeCreateViewModel vm);                      // returns new Id

        // UPDATE (profile only; does NOT change password)
        Task<bool> UpdateProfileAsync(EmployeeDetailsViewModel vm);          // true if updated

        // UPDATE PASSWORD (only password)
        Task<bool> ChangePasswordAsync(PasswordChangeViewModel vm);                // true if updated

        // DELETE
        Task<bool> DeleteAsync(string id);
    }
}
