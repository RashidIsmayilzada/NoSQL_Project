using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces;

public interface ITicketRepository
{
    Task<List<Ticket>> GetAllTickets();
    Task<Ticket> GetTicketById(string id);
    Task CreateTicket(Ticket ticket);
    Task UpdateTicket(string id, Ticket ticket);
    Task DeleteTicket(string id);
    Task<IEnumerable<Ticket>> GetByReporterIdAsync(string userId);
    Task<bool> AssignAsync(string ticketId, string assigneeUserId);

    // Returns all tickets that are currently assigned to a specific user (by userId)
    Task<IEnumerable<Ticket>> GetAssignedToUserAsync(string userId);
    //Task<bool> AssignTicketToEmployeeAsync(string ticketId, string employeeId);
}