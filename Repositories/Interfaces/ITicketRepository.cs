using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces;

public interface ITicketRepository
{
    Task<List<Ticket>> GetAllTickets();
    Task<Ticket> GetTicketById(string id);
    Task CreateTicket(Ticket ticket);
    Task UpdateTicket(string id, Ticket ticket);
    Task DeleteTicket(string id);
    Task<int> GetTotalTicketsCountAsync();
    Task<int> GetUnresolvedTicketsCountAsync();
    Task<int> GetTicketsPastDeadlineCountAsync();
}