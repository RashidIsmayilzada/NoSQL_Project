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
      } */
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
    }

}