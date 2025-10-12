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
        /*   [HttpGet]
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
           } */
        // GET: /Ticket/Create
        [HttpGet]
        // GET: /Ticket/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new Ticket
            {
                Status = NoSQL_Project.Models.Enums.TicketStatus.Open, // شروع همه تیکت‌ها Open
                Priority = NoSQL_Project.Models.Enums.TicketPriority.Medium,

                Deadline = "7 days",                   // مطابق وایرفریم
                HandledBy = new List<HandlingInfo>()   // جلوگیری از null
                                                       // ReportedBy را فعلاً از فرم نمی‌گیریم (بعداً از کاربر لاگین‌شده ست می‌کنیم)
            };
            return View(model);
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

                // فیلدهایی که از فرم بایند نمی‌شن را از ModelState حذف کن
                ModelState.Remove("ReportedBy");
                ModelState.Remove("HandledBy[*].Employee"); // چون [BsonIgnore] است و در فرم نمی‌آید

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ticket creation");
                    return View(ticket);
                }

                // مقداردهی مطمئن قبل از ذخیره
                ticket.Id = null; // اجازه بده Mongo ObjectId بسازد

                if (string.IsNullOrWhiteSpace(ticket.TicketId))
                    ticket.TicketId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

                // Trim رشته‌ها
                ticket.Title = ticket.Title?.Trim() ?? "";
                ticket.Description = ticket.Description?.Trim() ?? "";
                // Type و Priority چون Enum هستند نیازی به Trim ندارند
                ticket.Deadline = ticket.Deadline?.Trim() ?? "7 days";

                // وضعیت پیش‌فرض
                ticket.Status = NoSQL_Project.Models.Enums.TicketStatus.Open;

                // لیست‌ها نال نباشند
                ticket.HandledBy ??= new List<HandlingInfo>();

                _logger.LogInformation("Creating new ticket (title: {Title})", ticket.Title);
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
                if (string.IsNullOrWhiteSpace(id))
                {
                    TempData["Error"] = "Ticket ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ObjectId.TryParse(id, out _))
                {
                    TempData["Error"] = "Invalid ticket ID format.";
                    return RedirectToAction(nameof(Index));
                }

                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = $"Ticket with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                // اطمینان از نال نبودن لیست‌ها
                ticket.HandledBy ??= new List<HandlingInfo>();
                return View(ticket);
            }
            catch (MongoException)
            {
                TempData["Error"] = "Unable to load ticket. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Ticket/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Ticket ticket, string? reportedById) // ← حتماً ? داشته باشه
        {
            try
            {
                // اگر از فرم نیامد، از route ست می‌کنیم
                if (string.IsNullOrWhiteSpace(ticket?.Id))
                    ticket.Id = id;

                if (string.IsNullOrWhiteSpace(ticket.Id) || !ObjectId.TryParse(ticket.Id, out _))
                {
                    ModelState.AddModelError("", "Invalid ticket ID.");
                    return View(ticket);
                }

                // فیلدهایی که توی فرم بایند نمی‌شوند را از ModelState حذف کن
                ModelState.Remove("ReportedBy");
                var handledByKeys = ModelState.Keys
                    .Where(k => k.StartsWith("HandledBy[") && k.EndsWith(".Employee"))
                    .ToList();
                foreach (var k in handledByKeys) ModelState.Remove(k);

                if (!ModelState.IsValid)
                    return View(ticket);

                // اگر مقدار گزارش‌دهنده رسید، نگهش داریم
                if (!string.IsNullOrWhiteSpace(reportedById))
                    ticket.ReportedBy = new Employee { Id = reportedById };

                ticket.HandledBy ??= new List<HandlingInfo>();

                await _ticketService.UpdateTicketAsync(ticket.Id, ticket);

                TempData["Success"] = "Ticket updated successfully!";
                return RedirectToAction(nameof(Index)); // ✅ بعد از موفقیت برگرد به لیست
            }
            catch (MongoException)
            {
                ModelState.AddModelError("", "Database error. Please try again later.");
                return View(ticket);
            }
            catch
            {
                ModelState.AddModelError("", "Unexpected error occurred.");
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