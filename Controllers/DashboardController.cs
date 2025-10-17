using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels;
using System.Security.Claims;

namespace NoSQL_Project.Controllers
{

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboard;

        public DashboardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        public async Task<IActionResult> Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isServiceDesk = role == RoleType.ServiceDesk.ToString();

            var scope = role == RoleType.ServiceDesk.ToString()
                ? DashboardScope.AllTickets
                : DashboardScope.MyTickets;

            var breakdown = await _dashboard.GetStatusBreakdownAsync(scope, userId);
            var (total, open, overdue) = await _dashboard.GetOpenAndOverdueAsync(scope, userId);

            var vm = new DashboardVM
            {
                IsServiceDesk = isServiceDesk,
                StatusBreakdown = breakdown,
                TotalTickets = total,
                OpenTickets = open,
                OverdueOpen = overdue
            };

            return View(vm);
        }
    }
}
