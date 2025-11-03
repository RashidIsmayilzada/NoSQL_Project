using Humanizer;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;

    public TicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        return await _ticketRepository.GetAllTickets();
    }

    public async Task<Ticket> GetTicketByIdAsync(string id)
    {
        return await _ticketRepository.GetTicketById(id);
    }

    public async Task CreateTicketAsync(Ticket ticket)
    {
        await _ticketRepository.CreateTicket(ticket);
    }

    public async Task UpdateTicketAsync(string id, Ticket ticket)
    {
        await _ticketRepository.UpdateTicket(id, ticket);
    }

    public async Task DeleteTicketAsync(string id)
    {
        await _ticketRepository.DeleteTicket(id);
    }


    public Task<IEnumerable<Ticket>> GetForUserAsync(string userId)
    => _ticketRepository.GetByReporterIdAsync(userId);
    public async Task<bool> AssignAsync(string ticketId, string assigneeUserId)
    {
        return await _ticketRepository.AssignAsync(ticketId, assigneeUserId);
    }

    // Added a new feature to support the “My Tickets” page for ServiceDesk users.
    // This includes a new method GetAssignedToUserAsync in both Repository and Service layers,
    //which returns all tickets currently assigned to the logged-in user.
    public async Task<IEnumerable<Ticket>> GetAssignedToUserAsync(string userId)
    {
        return await _ticketRepository.GetAssignedToUserAsync(userId);
    }
    public async Task<bool> AssignTicketAsync(string ticketId, string employeeId)
    {
        return await _ticketRepository.AssignTicketToEmployeeAsync(ticketId, employeeId);
    }



}