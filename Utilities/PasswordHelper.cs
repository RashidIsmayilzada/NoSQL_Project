using System.Text;

namespace NoSQL_Project.Utilities
{
    public static class PasswordHelper
    {
        private const int WorkFactor = 10;

        public static string HashPassword(string password)
        {
            var pwd = password.Normalize(NormalizationForm.FormC);
            return BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: WorkFactor);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var pwd = password.Normalize(NormalizationForm.FormC);
            return BCrypt.Net.BCrypt.Verify(pwd, hashedPassword);
        }
    }
}
