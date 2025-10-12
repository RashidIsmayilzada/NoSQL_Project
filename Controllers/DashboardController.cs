using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    public class DashboardController : Controller
    {

        private readonly ITicketService _ticketService;
        public DashboardController(ITicketService ticketService) => _ticketService = ticketService;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // اگر “داشبورد من” می‌خواستی، اینجا می‌تونستیم reportedBy رو هم پاس بدیم
            var vm = await _ticketService.GetDashboardAsync(null);
            return View(vm); // Views/Dashboard/Index.cshtml با @model DashboardViewModel
        }
    }
}

