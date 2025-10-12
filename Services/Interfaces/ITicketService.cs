using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces;

public interface ITicketService
{
    Task<List<Ticket>> GetAllTicketsAsync();
    Task<Ticket> GetTicketByIdAsync(string id);
    Task CreateTicketAsync(Ticket ticket);
    Task UpdateTicketAsync(string id, Ticket ticket);
    Task DeleteTicketAsync(string id);
    Task AssignTicketAsync(string id, Ticket ticket);
    //  Task<(int total, int unresolved, int pastDeadline)> GetDashboardStatisticsAsync();

    Task<DashboardViewModel> GetDashboardAsync(string? reportedByEmployeeObjectId = null);
}