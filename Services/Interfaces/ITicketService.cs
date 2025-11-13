using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces;

public interface ITicketService
{
    Task<List<Ticket>> GetAllTicketsAsync();
    Task<Ticket> GetTicketByIdAsync(string id);
    Task CreateTicketAsync(Ticket ticket);
    Task UpdateTicketAsync(string id, Ticket ticket);
    Task DeleteTicketAsync(string id);
    Task<IEnumerable<Ticket>> GetForUserAsync(string userId);
    Task<bool> AssignAsync(string ticketId, string assigneeUserId);

    // Service layer: Gets all tickets assigned to a specific user
    Task<IEnumerable<Ticket>> GetAssignedToUserAsync(string userId);
   // Task<bool> AssignTicketAsync(string ticketId, string employeeId);


}