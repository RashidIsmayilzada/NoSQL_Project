using NoSQL_Project.Models.Enums;
using NoSQL_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.ViewModels.Employee
{
    public class EmployeeCreateViewModel
    {
        [Required]
        public bool IsDisabled { get; set; }
        [Required]
        public Name Name { get; set; }
        [Required]
        public RoleType Role { get; set; }
        [Required]
        public ContactInfo ContactInfo { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
