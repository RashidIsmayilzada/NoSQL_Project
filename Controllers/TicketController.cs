using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers;

public class TicketController
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    // GET: /Ticket
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        List<Ticket> tickets = await _ticketService.GetAllTicketsAsync();
        return new OkObjectResult(tickets);
    }

    // GET: /Ticket/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return new NotFoundResult();

        Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
        return new OkObjectResult(ticket);
    }

    // POST: /Ticket/Create
    [HttpPost]
    public async Task<IActionResult> Create(Ticket ticket)
    {
        if (ticket == null) return new BadRequestResult();

        await _ticketService.CreateTicketAsync(ticket);
        return new OkResult();
    }

    // PUT: /Ticket/Edit/{id}
    [HttpPut]
    public async Task<IActionResult> Edit(string id, Ticket ticket)
    {
        if (string.IsNullOrEmpty(id) || ticket == null) return new BadRequestResult();

        await _ticketService.UpdateTicketAsync(id, ticket);
        return new OkResult();
    }

    // DELETE: /Ticket/Delete/{id}
    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return new BadRequestResult();

        await _ticketService.DeleteTicketAsync(id);
        return new OkResult();
    }
}