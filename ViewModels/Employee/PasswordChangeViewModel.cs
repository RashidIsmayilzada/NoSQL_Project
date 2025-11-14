using NoSQL_Project.Models;

namespace NoSQL_Project.ViewModels.Employee
{
    public class PasswordChangeViewModel
    {
        public string Id { get; set; } = "";

        public Name Name { get; set; } = new Name();
        public string NewPassword { get; set; } = "";
        public string ConfirmNewPassword { get; set; } = "";
    }
}
