using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using NoSQL_Project.Services.Interfaces;


namespace NoSQL_Project.Services
{
    public class PasswordResetTokenService : IPasswordResetTokenService
    {
        private readonly ITimeLimitedDataProtector _protector;

        public PasswordResetTokenService(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider
                .CreateProtector("PasswordReset")
                .ToTimeLimitedDataProtector();
        }

        public string GenerateToken(string userId)
        {
            var bytes = Encoding.UTF8.GetBytes(userId);

            var protectedBytes = _protector.Protect(bytes, lifetime: TimeSpan.FromMinutes(5));

            return WebEncoders.Base64UrlEncode(protectedBytes);
        }

        public string? ValidateToken(string token)
        {
            try
            {
                var protectedBytes = WebEncoders.Base64UrlDecode(token);
                var bytes = _protector.Unprotect(protectedBytes); // throws if expired / tampered

                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }
    }

}
