using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ITicketService _ticketService;

    public HomeController(ILogger<HomeController> logger, ITicketService ticketService)
    {
        _logger = logger;
        _ticketService = ticketService;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return View(); // Views/Home/Index.cshtml (بدون مدل)
    }

    /* public async Task<IActionResult> Index()
     {
         var stats = await _ticketService.GetDashboardStatisticsAsync();

         var viewModel = new DashboardViewModel
         {
             TotalTickets = stats.total,
             UnresolvedTickets = stats.unresolved,
             TicketsPastDeadline = stats.pastDeadline
         };

         return View(viewModel);
     } */


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
