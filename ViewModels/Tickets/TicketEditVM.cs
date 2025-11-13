using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.ViewModels.Tickets
{
    public class TicketEditVM
    {
        [Required]
        public string Id { get; set; } = string.Empty; // MongoDB Ticket ID

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Priority")]
        public string? Priority { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Assign To")]
        public string? HandledById { get; set; } // ID of the person assigned to handle the ticket

        public IEnumerable<SelectListItem>? EmployeeOptions { get; set; } // For dropdown
    }

}
