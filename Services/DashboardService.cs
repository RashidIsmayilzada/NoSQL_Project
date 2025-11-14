using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels.Dashboard;

namespace NoSQL_Project.Services
{

    public class DashboardService : IDashboardService
    {
        private readonly IMongoCollection<Ticket> _tickets;

        public DashboardService(IMongoDatabase db)
        {
            _tickets = db.GetCollection<Ticket>("Ticket");
        }

        private FilterDefinition<Ticket> ScopeFilter(DashboardScope scope, string? userId)
        {
            var b = Builders<Ticket>.Filter;
            return scope == DashboardScope.MyTickets && !string.IsNullOrEmpty(userId)
                ? b.Eq(t => t.ReportedBy, userId)
                : FilterDefinition<Ticket>.Empty;
        }

        public async Task<DonutDataset> GetStatusBreakdownAsync(DashboardScope scope, string? userId)
        {
            var match = ScopeFilter(scope, userId);

            // group by Status
            var pipeline = _tickets.Aggregate()
                .Match(match)
                .Group(t => t.Status, g => new { Status = g.Key, Count = g.Count() });

            var rows = await pipeline.ToListAsync();

            // ترتیب دلخواه وضعیت‌ها
            var order = new[] { TicketStatus.Open, TicketStatus.InProgress, TicketStatus.OnHold, TicketStatus.Resolved, TicketStatus.Closed };

            var labels = new List<string>();
            var data = new List<int>();
            var total = 0;

            foreach (var st in order)
            {
                var row = rows.FirstOrDefault(r => r.Status == st);
                var c = row?.Count ?? 0;
                labels.Add(st.ToString());
                data.Add(c);
                total += c;
            }

            return new DonutDataset(labels, data, total);
        }

        public async Task<(int total, int open, int overdue)> GetOpenAndOverdueAsync(DashboardScope scope, string? userId)
        {
            var b = Builders<Ticket>.Filter;
            var baseFilter = ScopeFilter(scope, userId);

            var total = (int)await _tickets.CountDocumentsAsync(baseFilter);

            var openFilter = b.And(baseFilter, b.Eq(t => t.Status, TicketStatus.Open));
            var open = (int)await _tickets.CountDocumentsAsync(openFilter);

            // --- اگر Deadline = DateTime: (پیشنهادی) ---
            // var now = DateTime.UtcNow;
            // var overdueFilter = b.And(openFilter, b.Lt(t => t.DeadlineDateTime, now));
            // var overdue = (int)await _tickets.CountDocumentsAsync(overdueFilter);

            // --- اگر فعلاً Deadline = string است: (راه‌حل موقت) ---
            // فرض: رشته‌ها ISO-8601 هستند (مثلاً "2025-10-17T12:00:00Z") تا بشود مقایسه کرد
            var nowIso = DateTime.UtcNow.ToString("o"); // ISO 8601
            var overdueFilter = b.And(
                openFilter,
                b.Lt(nameof(Ticket.Deadline), nowIso) // مقایسهٔ لغوی روی ISO string
            );
            var overdue = (int)await _tickets.CountDocumentsAsync(overdueFilter);

            return (total, open, overdue);
        }
    }
}