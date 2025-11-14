using MongoDB.Bson;
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
        var filter = Builders<Ticket>.Filter.Empty;

        var tickets = await _tickets
            .Find(filter)
            .SortByDescending(t => t.TicketId)
            .ToListAsync();

        return tickets;
    }

    public async Task<Ticket> GetTicketById(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            throw new ArgumentException("Invalid ObjectId format", nameof(id));

        var filter = Builders<Ticket>.Filter.Eq("_id", objectId);
        var ticket = await _tickets.Find(filter).FirstOrDefaultAsync();

        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with id {id} not found.");

        return ticket;
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
    public async Task<IEnumerable<Ticket>> GetByReporterIdAsync(string userId)
    {
        var filter = Builders<Ticket>.Filter.Eq(t => t.ReportedBy, userId);
        return await _tickets.Find(filter).ToListAsync();
    }


    // Retrieves all tickets assigned to the given user, It’s used for showing the “My Tickets” list for ServiceDesk employees
    public async Task<IEnumerable<Ticket>> GetAssignedToUserAsync(string userId)
    {
        var filter = Builders<Ticket>.Filter.Eq(t => t.AssignedTo, userId);
        return await _tickets.Find(filter).ToListAsync();
    }

    public async Task<bool> AssignAsync(string ticketId, string assigneeUserId)
    {
        var filter = Builders<Ticket>.Filter.Eq(t => t.Id, ticketId);

        var update = Builders<Ticket>.Update
            .Set(t => t.AssignedTo, assigneeUserId);

        var res = await _tickets.UpdateOneAsync(filter, update);

        return res.MatchedCount == 1;
    }



}
