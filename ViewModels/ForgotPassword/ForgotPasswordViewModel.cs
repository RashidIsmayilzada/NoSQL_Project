using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.ViewModels.ForgotPassword
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
