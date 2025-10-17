using NoSQL_Project.Models.Enums;
using NoSQL_Project.ViewModels;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DonutDataset> GetStatusBreakdownAsync(DashboardScope scope, string? userId);
        Task<(int total, int open, int overdue)> GetOpenAndOverdueAsync(DashboardScope scope, string? userId);
    }
}
