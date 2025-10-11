using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly IMongoCollection<Ticket> _tickets;

    public TicketRepository(IMongoDatabase database)
    {
        _tickets = database.GetCollection<Ticket>("Ticket");
    }
    public async Task<List<Ticket>> GetAllTickets()
    {
        return await _tickets.Find(FilterDefinition<Ticket>.Empty).ToListAsync();
    }
    
    public async Task<Ticket> GetTicketById(string id)
    {
        return await _tickets.Find(ticket => ticket.Id == id).FirstOrDefaultAsync();
    }
    
    public async Task CreateTicket(Ticket ticket)
    {
        await _tickets.InsertOneAsync(ticket);
    }
    
    public async Task UpdateTicket(string id, Ticket ticket)
    {
        await _tickets.ReplaceOneAsync(t => t.Id == id, ticket);
    }
    
    public async Task DeleteTicket(string id)
    {
        await _tickets.DeleteOneAsync(t => t.Id == id);
    }
}