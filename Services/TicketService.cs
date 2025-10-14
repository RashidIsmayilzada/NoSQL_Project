using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
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

    public async Task AssignTicketAsync(string id, Ticket ticket)
    {
        await _ticketRepository.UpdateTicket(id, ticket);
    }

    /*  public async Task<(int total, int unresolved, int pastDeadline)> GetDashboardStatisticsAsync()
      {
          var totalTask = _ticketRepository.GetTotalTicketsCountAsync();
          var unresolvedTask = _ticketRepository.GetUnresolvedTicketsCountAsync();
          var pastDeadlineTask = _ticketRepository.GetTicketsPastDeadlineCountAsync();

          await Task.WhenAll(totalTask, unresolvedTask, pastDeadlineTask);

          return (totalTask.Result, unresolvedTask.Result, pastDeadlineTask.Result);
      } 
    public async Task<DashboardViewModel> GetDashboardAsync(string? reportedByEmployeeObjectId = null)
    {
        var counts = await _ticketRepository.GetStatusCountsAsync(reportedByEmployeeObjectId);

        int open = 0, resolved = 0, closed = 0;

        foreach (var kv in counts)
        {
            var status = kv.Key.ToLower();
            if (status == "open" || status == "inprogress" || status == "onhold")
                open += kv.Value;
            else if (status == "resolved")
                resolved += kv.Value;
            else if (status == "closed")
                closed += kv.Value;
        }

        return new DashboardViewModel
        {
            Total = open + resolved + closed,
            OpenCount = open,
            ResolvedCount = resolved,
            ClosedWithoutResolveCount = closed
        };
    }*/

    public async Task<DashboardViewModel> GetDashboardAsync(string? reportedByEmployeeObjectId = null)
    {
        // روش ساده: همه تیکت‌ها را بگیر و در سرویس بشمار
        var tickets = await _ticketRepository.GetAllTickets();

        if (!string.IsNullOrWhiteSpace(reportedByEmployeeObjectId))
        {
            // اگر می‌خواهی فقط تیکت‌های یک کارمند خاص را نشان دهی (برای داشبوردِ کاربر معمولی)
            tickets = tickets
                .Where(t => t.ReportedBy?.Id == reportedByEmployeeObjectId)
                .ToList();
        }

        var total = tickets.Count;

        // enum های تو: Open, InProgress, OnHold, Resolved, Closed
        var open = tickets.Count(t =>
            t.Status == TicketStatus.Open ||
            t.Status == TicketStatus.InProgress ||
            t.Status == TicketStatus.OnHold);

        var resolved = tickets.Count(t => t.Status == TicketStatus.Resolved);
        var closed = tickets.Count(t => t.Status == TicketStatus.Closed);
        var pastDeadline = await _ticketRepository.GetTicketsPastDeadlineCountAsync();
        return new DashboardViewModel
        {
            Total = total,
            OpenCount = open,
            ResolvedCount = resolved,
            ClosedWithoutResolveCount = closed,
            TicketsPastDeadline = pastDeadline
        };
    }

}
