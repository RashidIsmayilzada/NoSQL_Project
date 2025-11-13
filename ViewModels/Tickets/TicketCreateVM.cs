
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

    // فعلاً string چون مدل دیتابیس هم string دارد
    [Required]
    public string Deadline { get; set; } = "";

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";

    // فقط برای ServiceDesk: انتخاب گزارش‌دهنده
    public string? ReportedBy { get; set; }

    // Dropdown کاربران (فقط Id و نام)
    public IEnumerable<SelectListItem> ReporterOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
// فقط برای ServiceDesk نمایش داده می‌شود:
