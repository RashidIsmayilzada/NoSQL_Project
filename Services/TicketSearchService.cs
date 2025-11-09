using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;
using System.Text.RegularExpressions;

namespace NoSQL_Project.Services
{
    /// <summary>
    /// Implements a simple AND/OR parser and builds a MongoDB filter over Title + Description.
    /// Sorting is newest first by ObjectId (works even if CreatedAt does not exist).
    /// </summary>
    /// Individual feature ticket search service Pariya Hallaji

    public class TicketSearchService : ITicketSearchService
    {
        private readonly IMongoCollection<Ticket> _tickets;

        public TicketSearchService(IMongoDatabase database)
        {
            _tickets = database.GetCollection<Ticket>("Ticket");
        }

        public async Task<IReadOnlyList<Ticket>> SearchAsync(string query, bool myScopeOnly, string userId, bool isServiceDesk)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<Ticket>();

            // --- Build text filter (Title/Description) with AND/OR ---
            var textFilter = BuildAndOrFilter(Normalize(query));

            // --- Scope filter ---
            FilterDefinition<Ticket> scopeFilter = FilterDefinition<Ticket>.Empty;

            if (myScopeOnly)
            {
                if (isServiceDesk)
                {
                    // My tickets (ServiceDesk): assigned to me OR handled by me (history)
                    var assignedToMe = Builders<Ticket>.Filter.Eq(t => t.AssignedTo, userId);
                    var handledByMe = Builders<Ticket>.Filter.ElemMatch(t => t.HandledBy,
                        Builders<HandlingInfo>.Filter.Eq(h => h.EmployeeId, userId));

                    scopeFilter = Builders<Ticket>.Filter.Or(assignedToMe, handledByMe);
                }
                else
                {
                    // Normal user: only tickets I reported
                    scopeFilter = Builders<Ticket>.Filter.Eq(t => t.ReportedBy, userId);
                }
            }

            var finalFilter = Builders<Ticket>.Filter.And(textFilter, scopeFilter);

            // --- Sort newest first (by ObjectId) ---
            var sort = Builders<Ticket>.Sort.Descending("_id");

            var results = await _tickets
                .Find(finalFilter)
                .Sort(sort)
                .ToListAsync();

            return results;
        }

        // -------------------- Helpers --------------------

        private static string Normalize(string input)
        {
            // Trim, collapse spaces, unify AND/OR to uppercase tokens
            var s = Regex.Replace(input ?? string.Empty, @"\s+", " ").Trim();
            s = Regex.Replace(s, @"\band\b", "AND", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\bor\b", "OR", RegexOptions.IgnoreCase);
            return s;
        }

        /// <summary>
        /// Parse "foo AND bar OR baz" into ( (foo AND bar) OR (baz) )
        /// Terms become case-insensitive regex over Title OR Description.
        /// </summary>
        private static FilterDefinition<Ticket> BuildAndOrFilter(string q)
        {
            var f = Builders<Ticket>.Filter;

            // Split by OR (top-level)
            var orGroups = q.Split(" OR ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var orFilters = new List<FilterDefinition<Ticket>>();

            foreach (var group in orGroups)
            {
                // Each group: split by AND
                var andTerms = group.Split(" AND ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var andFilters = new List<FilterDefinition<Ticket>>();

                foreach (var term in andTerms)
                {
                    if (string.IsNullOrWhiteSpace(term)) continue;

                    // Case-insensitive regex
                    var regex = new BsonRegularExpression(Regex.Escape(term), "i");

                    // (Title like term) OR (Description like term)
                    var inTitle = f.Regex(t => t.Title, regex);
                    var inDesc = f.Regex(t => t.Description, regex);
                    var oneTermFilter = f.Or(inTitle, inDesc);

                    andFilters.Add(oneTermFilter);
                }

                if (andFilters.Count == 0) continue;

                // AND all terms in this group
                var groupFilter = andFilters.Count == 1 ? andFilters[0] : f.And(andFilters);
                orFilters.Add(groupFilter);
            }

            if (orFilters.Count == 0)
            {
                // Fallback: no valid terms -> match nothing
                return f.Where(_ => false);
            }

            // OR all groups
            return orFilters.Count == 1 ? orFilters[0] : f.Or(orFilters);
        }
    }

}
