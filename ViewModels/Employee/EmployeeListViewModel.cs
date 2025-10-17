using NoSQL_Project.Models.Enums;
using NoSQL_Project.Models;

namespace NoSQL_Project.ViewModels.Employee
{
    public class EmployeeListViewModel
    {
        public string Id { get; set; } = "";
        public bool IsDisabled { get; set; } = false;
        public Name Name { get; set; } = new Name();
        public RoleType Role { get; set; }
        public string Email { get; set; } = "";
        public int ReportedTicketCount { get; set; } = 0;
    }
}
