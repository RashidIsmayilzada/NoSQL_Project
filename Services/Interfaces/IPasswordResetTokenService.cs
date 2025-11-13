namespace NoSQL_Project.Services.Interfaces
{
    public interface IPasswordResetTokenService
    {
        string GenerateToken(string userId);
        string? ValidateToken(string token);
    }
}
