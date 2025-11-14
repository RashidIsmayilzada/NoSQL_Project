using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.ViewModels.ForgotPassword
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
