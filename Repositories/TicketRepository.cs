using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
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
                { "as", "ReportedBy" },
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
                { "path", "$ReportedBy" },
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
                { "as", "ReportedBy" },
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
                { "path", "$ReportedBy" },
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
        // Create a BsonDocument to update, storing only the Employee ID in ReportedBy field
        var update = Builders<Ticket>.Update
            .Set(t => t.TicketId, ticket.TicketId)
            .Set(t => t.Title, ticket.Title)
            .Set(t => t.Type, ticket.Type)
            .Set(t => t.Priority, ticket.Priority)
            .Set(t => t.Status, ticket.Status)
            .Set(t => t.Deadline, ticket.Deadline)
            .Set(t => t.Description, ticket.Description)
            .Set(t => t.HandledBy, ticket.HandledBy);

        // Set ReportedBy as ObjectId (not the whole Employee object)
        if (ticket.ReportedBy != null && !string.IsNullOrEmpty(ticket.ReportedBy.Id))
        {
            var reportedByObjectId = new ObjectId(ticket.ReportedBy.Id);
            update = update.Set("ReportedBy", reportedByObjectId);
        }

        await _tickets.UpdateOneAsync(t => t.Id == id, update);
    }

    public async Task DeleteTicket(string id)
    {
        await _tickets.DeleteOneAsync(t => t.Id == id);
    }

    public async Task<int> GetTotalTicketsCountAsync()
    {
        return (int)await _tickets.CountDocumentsAsync(FilterDefinition<Ticket>.Empty);
    }

    public async Task<int> GetUnresolvedTicketsCountAsync()
    {
        var filter = Builders<Ticket>.Filter.And(
            Builders<Ticket>.Filter.Ne(t => t.Status, Models.Enums.TicketStatus.Closed),
            Builders<Ticket>.Filter.Ne(t => t.Status, Models.Enums.TicketStatus.Resolved)
        );
        return (int)await _tickets.CountDocumentsAsync(filter);
    }

    public async Task<int> GetTicketsPastDeadlineCountAsync()
    {
        // Use aggregation to get tickets with basic info (no need for ReportedBy population for counting)
        var pipeline = new[]
        {
            new BsonDocument("$project", new BsonDocument
            {
                { "Deadline", 1 },
                { "Status", 1 }
            })
        };

        var tickets = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();
        var today = DateTime.Now;

        int count = 0;
        foreach (var ticket in tickets)
        {
            if (ticket.Contains("Deadline") && !ticket["Deadline"].IsBsonNull)
            {
                var deadlineStr = ticket["Deadline"].AsString;
                if (!string.IsNullOrEmpty(deadlineStr))
                {
                    if (DateTime.TryParse(deadlineStr, out DateTime deadline))
                    {
                        var status = ticket["Status"].AsString;
                        if (deadline < today &&
                            status != "Closed" &&
                            status != "Resolved")
                        {
                            count++;
                        }
                    }
                }
            }
        }

        return count;
    }
    // آمار وضعیت‌ها برای داشبورد (همه‌ی تیکت‌ها یا فقط تیکت‌های یک کارمند)
    // خروجی: map از Status به Count
    public async Task<Dictionary<string, int>> GetStatusCountsAsync(string? reportedByEmployeeObjectId = null)
    {
        var match = reportedByEmployeeObjectId == null
            ? FilterDefinition<Ticket>.Empty
            : Builders<Ticket>.Filter.Eq("ReportedBy", new ObjectId(reportedByEmployeeObjectId));

        var matchStage = new BsonDocument
    {
        { "$match", match.ToBsonDocument() }
    };

        var groupStage = new BsonDocument
    {
        { "$group", new BsonDocument
            {
                { "_id", "$Status" },
                { "count", new BsonDocument("$sum", 1) }
            }
        }
    };

        var pipeline = new[] { matchStage, groupStage };

        var docs = await _tickets.Aggregate<BsonDocument>(pipeline).ToListAsync();

        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in docs)
        {
            var key = d["_id"].IsBsonNull ? "" : d["_id"].AsString;
            var val = d["count"].ToInt32();
            dict[key] = val;
        }
        return dict;
    }
   /* public async Task<(int Total, int Open, int Resolved, int Closed)>
        GetDashboardCountsAsync(string? reportedByEmployeeObjectId)
    {
        var baseFilter = FilterDefinition<Ticket>.Empty;

        // اگر داشبورد شخصی (My dashboard) خواستی:
        if (!string.IsNullOrWhiteSpace(reportedByEmployeeObjectId))
        {
            var byMe = Builders<Ticket>.Filter.Eq("ReportedBy", new MongoDB.Bson.ObjectId(reportedByEmployeeObjectId));
            baseFilter = byMe;
        }

        // Unresolved = Open + InProgress + OnHold
        var unresolvedFilter = Builders<Ticket>.Filter.In<Ticket, TicketStatus>(
            t => t.Status, new[] { TicketStatus.Open, TicketStatus.InProgress, TicketStatus.OnHold });

        var resolvedFilter = Builders<Ticket>.Filter.Eq<Ticket, TicketStatus>(t => t.Status, TicketStatus.Resolved);
        var closedFilter = Builders<Ticket>.Filter.Eq<Ticket, TicketStatus>(t => t.Status, TicketStatus.Closed);

        // countها
        var total = (int)await _tickets.CountDocumentsAsync(baseFilter);
        var open = (int)await _tickets.CountDocumentsAsync(Builders<Ticket>.Filter.And(baseFilter, unresolvedFilter));
        var resolved = (int)await _tickets.CountDocumentsAsync(Builders<Ticket>.Filter.And(baseFilter, resolvedFilter));
        var closed = (int)await _tickets.CountDocumentsAsync(Builders<Ticket>.Filter.And(baseFilter, closedFilter));

        return (total, open, resolved, closed);
    } */
}

