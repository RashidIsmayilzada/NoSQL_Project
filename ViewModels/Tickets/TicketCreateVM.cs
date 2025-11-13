
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoSQL_Project.Models.Enums;
namespace NoSQL_Project.ViewModels.Tickets;

public class TicketCreateVM
{
    [Required, StringLength(120)]
    public string Title { get; set; } = "";

    [Required]
    public TicketType Type { get; set; }

    [Required]
    public TicketPriority Priority { get; set; }

    // Currently string because the database model also has string
    [Required]
    public string Deadline { get; set; } = "";

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";

    
    //Only for ServiceDesk: select reporter
    public string? ReportedBy { get; set; }

    // Dropdown users (only Id and Name)
    public IEnumerable<SelectListItem> ReporterOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
