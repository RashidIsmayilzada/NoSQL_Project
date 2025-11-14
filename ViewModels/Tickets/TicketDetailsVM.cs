using NoSQL_Project.Models.Enums;
namespace NoSQL_Project.ViewModels.Tickets;

public class TicketDetailsVM
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public TicketType Type { get; set; }
    public TicketPriority Priority { get; set; }
    public string Deadline { get; set; } = "";
    public string Description { get; set; } = "";
    public TicketStatus Status { get; set; }
    public string ReporterName { get; set; } = "";
    public string? AssigneeName { get; set; }
}