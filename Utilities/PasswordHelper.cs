namespace NoSQL_Project.Utilities
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            // Use a secure hashing algorithm like BCrypt or Argon2
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
