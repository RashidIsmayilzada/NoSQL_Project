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
        // Use aggregation pipeline to populate employee data WITHOUT ReportedTickets array
        var pipeline = new[]
        {
            // Stage 1: Lookup ReportedBy employee (project to exclude ReportedTickets)
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Employee" },
                { "localField", "ReportedBy" },
                { "foreignField", "_id" },
                { "as", "ReportedByEmployee" },
                { "pipeline", new BsonArray
                    {
                        // Project only fields we want (exclude ReportedTickets)
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "_id", 1 },
                            { "EmployeeId", 1 },
                            { "Name", 1 },
                            { "Role", 1 },
                            { "contactInfo", 1 }
                            // ReportedTickets array NOT included - prevents infinite loop
                        })
                    }
                }
            }),
            // Stage 2: Convert array to single object
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$ReportedByEmployee" },
                { "preserveNullAndEmptyArrays", true }
            })
        };

        return await _tickets.Aggregate<Ticket>(pipeline).ToListAsync();
    }
    
    public async Task<Ticket> GetTicketById(string id)
    {
        // Use aggregation pipeline for single ticket
        var pipeline = new[]
        {
            // Match specific ticket
            new BsonDocument("$match", new BsonDocument
            {
                { "_id", new ObjectId(id) }
            }),
            // Lookup ReportedBy employee
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Employee" },
                { "localField", "ReportedBy" },
                { "foreignField", "_id" },
                { "as", "ReportedByEmployee" },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "_id", 1 },
                            { "EmployeeId", 1 },
                            { "Name", 1 },
                            { "Role", 1 },
                            { "contactInfo", 1 }
                        })
                    }
                }
            }),
            // Convert array to single object
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$ReportedByEmployee" },
                { "preserveNullAndEmptyArrays", true }
            })
        };

        return await _tickets.Aggregate<Ticket>(pipeline).FirstOrDefaultAsync();
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
    public async Task<bool> AssignAsync(string ticketId, string assigneeUserId)
    {
        var filter = Builders<Ticket>.Filter.Eq(t => t.Id, ticketId);
        var update = Builders<Ticket>.Update.Set(t => t.AssignedTo, assigneeUserId);

        var res = await _tickets.UpdateOneAsync(filter, update);
        return res.MatchedCount == 1;
    }

    // Retrieves all tickets assigned to the given user, It’s used for showing the “My Tickets” list for ServiceDesk employees
    public async Task<IEnumerable<Ticket>> GetAssignedToUserAsync(string userId)
    {
        var filter = Builders<Ticket>.Filter.Eq(t => t.AssignedTo, userId);
        return await _tickets.Find(filter).ToListAsync();
    }
    public async Task<bool> AssignTicketToEmployeeAsync(string ticketId, string employeeId)
    {
        var filter = Builders<Ticket>.Filter.Eq(t => t.Id, ticketId);
        var update = Builders<Ticket>.Update.Push(t => t.HandledBy, new HandlingInfo
        {
            EmployeeId = employeeId,
            Date = DateTime.Now.ToString("yyyy-MM-dd")
        });

        var result = await _tickets.UpdateOneAsync(filter, update);
        return result.ModifiedCount == 1;
    }


}
