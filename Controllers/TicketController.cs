using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(ITicketService ticketService, ILogger<TicketController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        // GET: /Ticket
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Retrieving all tickets");
                var tickets = await _ticketService.GetAllTicketsAsync();
                return View(tickets);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while retrieving tickets");
                TempData["Error"] = "Unable to retrieve tickets. Please try again later.";
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving tickets");
                TempData["Error"] = "An unexpected error occurred.";
                return View("Error");
            }
        }

        // GET: /Ticket/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("Details called with null or empty ID");
                    TempData["Error"] = "Ticket ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("Invalid ticket ID format: {TicketId}", id);
                    TempData["Error"] = "Invalid ticket ID format.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Retrieving ticket with ID: {TicketId}", id);
                var ticket = await _ticketService.GetTicketByIdAsync(id);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket not found with ID: {TicketId}", id);
                    TempData["Error"] = $"Ticket with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(ticket);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while retrieving ticket {TicketId}", id);
                TempData["Error"] = "Unable to retrieve ticket. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving ticket {TicketId}", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Ticket/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Ticket/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            try
            {
                if (ticket == null)
                {
                    _logger.LogWarning("Create called with null ticket");
                    ModelState.AddModelError("", "Ticket data is required.");
                    return View(ticket);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ticket creation");
                    return View(ticket);
                }

                _logger.LogInformation("Creating new ticket: {TicketId}", ticket.TicketId);
                await _ticketService.CreateTicketAsync(ticket);

                TempData["Success"] = "Ticket created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while creating ticket");
                ModelState.AddModelError("", "Unable to create ticket. Please try again later.");
                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating ticket");
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(ticket);
            }
        }

        // GET: /Ticket/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("Edit called with null or empty ID");
                    TempData["Error"] = "Ticket ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("Invalid ticket ID format: {TicketId}", id);
                    TempData["Error"] = "Invalid ticket ID format.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Loading ticket for edit: {TicketId}", id);
                var ticket = await _ticketService.GetTicketByIdAsync(id);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket not found for edit with ID: {TicketId}", id);
                    TempData["Error"] = $"Ticket with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(ticket);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while loading ticket for edit {TicketId}", id);
                TempData["Error"] = "Unable to load ticket. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while loading ticket for edit {TicketId}", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Ticket/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Ticket ticket)
        {
            try
            {
                if (ticket == null)
                {
                    _logger.LogWarning("Edit POST called with null ticket");
                    TempData["Error"] = "Ticket data is required.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrEmpty(ticket.Id))
                {
                    _logger.LogWarning("Edit POST called with null or empty ticket ID");
                    ModelState.AddModelError("", "Ticket ID is required.");
                    return View(ticket);
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(ticket.Id, out _))
                {
                    _logger.LogWarning("Invalid ticket ID format in edit: {TicketId}", ticket.Id);
                    ModelState.AddModelError("", "Invalid ticket ID format.");
                    return View(ticket);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ticket edit: {TicketId}", ticket.Id);
                    return View(ticket);
                }

                _logger.LogInformation("Updating ticket: {TicketId}", ticket.Id);
                await _ticketService.UpdateTicketAsync(ticket.Id, ticket);

                TempData["Success"] = "Ticket updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while updating ticket {TicketId}", ticket?.Id);
                ModelState.AddModelError("", "Unable to update ticket. Please try again later.");
                return View(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating ticket {TicketId}", ticket?.Id);
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(ticket);
            }
        }

        // GET: /Ticket/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("Delete called with null or empty ID");
                    TempData["Error"] = "Ticket ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("Invalid ticket ID format: {TicketId}", id);
                    TempData["Error"] = "Invalid ticket ID format.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Loading ticket for delete confirmation: {TicketId}", id);
                var ticket = await _ticketService.GetTicketByIdAsync(id);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket not found for delete with ID: {TicketId}", id);
                    TempData["Error"] = $"Ticket with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(ticket);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while loading ticket for delete {TicketId}", id);
                TempData["Error"] = "Unable to load ticket. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while loading ticket for delete {TicketId}", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Ticket/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("DeleteConfirmed called with null or empty ID");
                    TempData["Error"] = "Ticket ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate ObjectId format
                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("Invalid ticket ID format in delete: {TicketId}", id);
                    TempData["Error"] = "Invalid ticket ID format.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Deleting ticket: {TicketId}", id);
                await _ticketService.DeleteTicketAsync(id);

                TempData["Success"] = "Ticket deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Database error while deleting ticket {TicketId}", id);
                TempData["Error"] = "Unable to delete ticket. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting ticket {TicketId}", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}