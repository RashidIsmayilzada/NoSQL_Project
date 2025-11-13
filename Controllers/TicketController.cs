using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels.Employee;
using NoSQL_Project.ViewModels.Tickets;
using System.Security.Claims;

namespace NoSQL_Project.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketSearchService _ticketSearchService; //individual feature ticket search service Pariya Hallaji
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(ITicketService ticketService, ITicketSearchService ticketSearchService, IEmployeeService employeeService, ILogger<TicketController> logger)
        {
            _ticketService = ticketService;
            _ticketSearchService = ticketSearchService;
            _employeeService = employeeService;
            _logger = logger;
        }

        // ---------- Helpers ----------
        //private static string? ResolveAssigneeId(Ticket t)
        //{
        //    return string.IsNullOrWhiteSpace(t.AssignedTo) ? null : t.AssignedTo;
        //}

        private async Task<string?> ResolveAssigneeNameAsync(Ticket t)
        {
            var assigneeId = t.AssignedTo;
            if (string.IsNullOrWhiteSpace(assigneeId)) return null;

            var emp = await _employeeService.GetDetailsAsync(assigneeId);
            return emp == null ? null : $"{emp.Name.FirstName} {emp.Name.LastName}";
        }

        private async Task<string?> ResolveReporterNameAsync(Ticket t)
        {
            var reporterId = t.ReportedBy;
            if (string.IsNullOrWhiteSpace(reporterId)) return null;

            var emp = await _employeeService.GetDetailsAsync(reporterId);
            return emp == null ? null : emp.Name.ToString();
        }

        private static string GetDisplayName(EmployeeListViewModel e)
        {
            var fullName = $"{e.Name?.FirstName} {e.Name?.LastName}".Trim();
            return !string.IsNullOrWhiteSpace(fullName) ? $"{fullName} ({e.Email})" : (e.Email ?? e.Id);
        }

        // ---------- Index ("My Tickets") ----------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var tickets = isDesk
                    ? await _ticketService.GetAssignedToUserAsync(userId)
                    : await _ticketService.GetForUserAsync(userId);

                var list = new List<TicketListItemVM>();
                foreach (var t in tickets)
                {
                    var assigneeName = await ResolveAssigneeNameAsync(t);
                    string reporterName = await ResolveReporterNameAsync(t);
                    list.Add(new TicketListItemVM
                    {
                        Id = t.Id!,
                        Title = t.Title,
                        Status = t.Status,
                        Priority = t.Priority,
                        Deadline = t.Deadline,
                        ReporterName = reporterName,
                        AssigneeName = assigneeName,
                        IsAssignedToCurrentUser =
                            (!string.IsNullOrEmpty(t.AssignedTo) && t.AssignedTo == userId)
                    });
                }

                var vm = new TicketListVM { Items = list, IsServiceDesk = isDesk };
                ViewBag.Scope = "My";
                return View("Index", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading My Tickets.");
                TempData["Error"] = "Failed to load your tickets.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ---------- All ("All Tickets" - ServiceDesk only) ----------
        [HttpGet]
        public async Task<IActionResult> All()
        {
            try
            {
                if (!User.IsInRole(nameof(RoleType.ServiceDesk)))
                {
                    TempData["Error"] = "Only ServiceDesk can view all tickets.";
                    return RedirectToAction(nameof(Index));
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tickets = await _ticketService.GetAllTicketsAsync();

                var list = new List<TicketListItemVM>();
                foreach (var t in tickets)
                {
                    var assigneeName = await ResolveAssigneeNameAsync(t);
                    string reporterName = await ResolveReporterNameAsync(t);
                    list.Add(new TicketListItemVM
                    {
                        Id = t.Id!,
                        Title = t.Title,
                        Status = t.Status,
                        Priority = t.Priority,
                        Deadline = t.Deadline,
                        ReporterName = reporterName,
                        AssigneeName = assigneeName,
                        IsAssignedToCurrentUser =
                            (!string.IsNullOrEmpty(t.AssignedTo) && t.AssignedTo == userId)
                    });
                }

                var vm = new TicketListVM { Items = list, IsServiceDesk = true };
                ViewBag.Scope = "All";
                return View("Index", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading All Tickets.");
                TempData["Error"] = "Failed to load all tickets.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- Details ----------
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
                {
                    TempData["Error"] = "Invalid ticket id.";
                    return RedirectToAction(nameof(Index));
                }

                var t = await _ticketService.GetTicketByIdAsync(id);
                if (t == null)
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction(nameof(Index));
                }

                var assigneeName = await ResolveAssigneeNameAsync(t);
                string reporterName = await ResolveReporterNameAsync(t);

                var vm = new TicketDetailsVM
                {
                    Id = t.Id!,
                    Title = t.Title,
                    Type = t.Type,
                    Priority = t.Priority,
                    Deadline = t.Deadline,
                    Description = t.Description,
                    Status = t.Status,
                    ReporterName = reporterName,
                    AssigneeName = assigneeName ?? "-"
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Details for {TicketId}", id);
                TempData["Error"] = "Failed to load ticket details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- Create (GET) ----------
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            try
            {
                var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
                var vm = new TicketCreateVM();

                if (isDesk)
                {
                    var employees = await _employeeService.GetListAsync();
                    vm.ReporterOptions = employees.Select(e => new SelectListItem(GetDisplayName(e), e.Id));
                }
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Create form.");
                TempData["Error"] = "Cannot load ticket form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- Create (POST) ----------
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateVM vm)
        {
            try
            {
                var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!ModelState.IsValid)
                {
                    if (isDesk)
                    {
                        var employees = await _employeeService.GetListAsync();
                        vm.ReporterOptions = employees.Select(e => new SelectListItem(GetDisplayName(e), e.Id));
                    }
                    return View(vm);
                }

                var ticket = new Ticket
                {
                    Title = vm.Title,
                    Type = vm.Type,
                    Priority = vm.Priority,
                    Deadline = vm.Deadline,
                    Description = vm.Description,
                    Status = TicketStatus.Open,
                    ReportedBy = isDesk ? vm.ReportedBy : currentUserId
                };

                await _ticketService.CreateTicketAsync(ticket);
                TempData["Success"] = "Ticket created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket.");
                TempData["Error"] = "Failed to create ticket.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- Edit (GET) ----------
        [HttpGet]
        [Authorize]   // Only login is required; role is checked within the action itself.

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
                {
                    TempData["Error"] = "Invalid ticket ID.";
                    return RedirectToAction(nameof(Index));
                }

                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction(nameof(Index));
                }

                
                var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!isDesk && ticket.ReportedBy != userId)
                {
                    TempData["Error"] = "You are not allowed to edit this ticket.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new TicketEditVM
                {
                    Id = ticket.Id!,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Priority = ticket.Priority.ToString(),
                    Status = ticket.Status.ToString(),
                    HandledById = ticket.AssignedTo // same helper above the controller
                };

                // Only ServiceDesk sees the assign list
                if (isDesk)
                {
                    var employees = await _employeeService.GetListAsync();
                    model.EmployeeOptions = employees.Select(e => new SelectListItem
                    {
                        Value = e.Id,
                        Text = $"{e.Name.FirstName} {e.Name.LastName}"
                    }).ToList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Edit form for {TicketId}", id);
                TempData["Error"] = "Cannot load edit form.";
                return RedirectToAction(nameof(Index));
            }
        }


        // ---------- Edit (POST) ----------
        [HttpPost]
        [Authorize] // only login; role/ownership is checked below
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketEditVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill in all required fields.";
                    // if ServiceDesk, rebuild the dropdown
                    if (User.IsInRole(nameof(RoleType.ServiceDesk)))
                    {
                        var employees = await _employeeService.GetListAsync();
                        model.EmployeeOptions = employees.Select(e => new SelectListItem
                        {
                            Value = e.Id,
                            Text = $"{e.Name.FirstName} {e.Name.LastName}"
                        }).ToList();
                    }
                    return View(model);
                }

                var ticket = await _ticketService.GetTicketByIdAsync(model.Id);
                if (ticket == null)
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Permission: ServiceDesk or the reporter
                var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!isDesk && ticket.ReportedBy != userId)
                {
                    TempData["Error"] = "You are not allowed to edit this ticket.";
                    return RedirectToAction(nameof(Index));
                }

                // Editable fields
                ticket.Title = model.Title;
                ticket.Description = model.Description;
                ticket.Priority = Enum.Parse<TicketPriority>(model.Priority);
                ticket.Status = Enum.Parse<TicketStatus>(model.Status);

                // only ServiceDesk can change the assignee
                if (isDesk && !string.IsNullOrEmpty(model.HandledById))
                {
                    ticket.AssignedTo ??= model.HandledById;
                    ticket.AssignedTo = model.HandledById;

                }

                await _ticketService.UpdateTicketAsync(ticket.Id!, ticket);
                TempData["Success"] = "Ticket updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", model.Id);
                TempData["Error"] = "Failed to update ticket.";
                return RedirectToAction(nameof(Index));
            }
        }


        // ---------- Assign (GET, pick any employee) ----------
        [HttpGet]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        public async Task<IActionResult> Assign(string id)
        {
            try
            {
                var employees = await _employeeService.GetListAsync();
                ViewBag.Employees = new SelectList(
                    employees.Select(e => new { e.Id, FullName = $"{e.Name.FirstName} {e.Name.LastName}" }),
                    "Id",
                    "FullName"
                );
                ViewBag.TicketId = id;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Assign popup for {TicketId}", id);
                TempData["Error"] = "Cannot load assign dialog.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- Assign (POST) ----------
        [HttpPost]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(string id, string assigneeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
                {
                    TempData["Error"] = "Invalid ticket id.";
                    return RedirectToAction(nameof(Index));
                }
                if (string.IsNullOrWhiteSpace(assigneeId))
                {
                    TempData["Error"] = "Please choose an assignee.";
                    return RedirectToAction(nameof(Index));
                }

                var ok = await _ticketService.AssignAsync(id, assigneeId);
                TempData[ok ? "Success" : "Error"] = ok ? "Assigned successfully." : "Assign failed.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket {TicketId} to {AssigneeId}", id, assigneeId);
                TempData["Error"] = "Unexpected error during assign.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- AssignToMe (POST) ----------
        [HttpPost]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToMe(string id, string? returnTo = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
                {
                    TempData["Error"] = "Invalid ticket id.";
                    return RedirectToAction(nameof(Index));
                }

                var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(myId))
                {
                    TempData["Error"] = "Cannot resolve current user.";
                    return RedirectToAction(nameof(Index));
                }

                var ok = await _ticketService.AssignAsync(id, myId);
                TempData[ok ? "Success" : "Error"] = ok ? "Assigned to you." : "Assign failed.";

                if (string.Equals(returnTo, "All", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction(nameof(All));

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error AssignToMe for {TicketId}", id);
                TempData["Error"] = "Unexpected error during assign to you.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- Close (GET) ----------
        [HttpGet]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        public async Task<IActionResult> Close(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
                {
                    TempData["Error"] = "Invalid ticket id.";
                    return RedirectToAction(nameof(Index));
                }

                var t = await _ticketService.GetTicketByIdAsync(id);
                if (t == null)
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View("Close", t);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Close for {TicketId}", id);
                TempData["Error"] = "Cannot load close confirmation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- Close (POST) ----------
        [HttpPost, ActionName("Close")]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
                {
                    TempData["Error"] = "Invalid ticket id.";
                    return RedirectToAction(nameof(Index));
                }

                var t = await _ticketService.GetTicketByIdAsync(id);
                if (t == null)
                {
                    TempData["Error"] = "Ticket not found.";
                    return RedirectToAction(nameof(Index));
                }

                //"Just set status to 'Closed'; we don't delete."
                t.Status = TicketStatus.Closed;
                await _ticketService.UpdateTicketAsync(t.Id!, t);

                TempData["Success"] = "Ticket closed.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing {TicketId}", id);
                TempData["Error"] = "Unexpected error while closing ticket.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ---------- Search (GET) ----------
        //Individual feature ticket search service Pariya Hallaji

        [HttpGet]
        public async Task<IActionResult> Search(string q, string? scope = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return RedirectToAction(nameof(Index));

                var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // If ServiceDesk + scope==All => search all, else search only "my" scope
                bool myScopeOnly = !(isDesk && string.Equals(scope, "All", StringComparison.OrdinalIgnoreCase));

                var tickets = await _ticketSearchService.SearchAsync(q, myScopeOnly, userId, isDesk);

                // Map to VM (reuse the controller helper to keep things DRY)
                var list = new List<TicketListItemVM>();
                foreach (var t in tickets)
                {
                    //  Useی the helper so logic stays consistent with Index/All
                    var assigneeName = await ResolveAssigneeNameAsync(t);
                    string reporterName = await ResolveReporterNameAsync(t);

                    list.Add(new TicketListItemVM
                    {
                        Id = t.Id!,
                        Title = t.Title,
                        Status = t.Status,
                        Priority = t.Priority,
                        Deadline = t.Deadline,
                        ReporterName = reporterName,
                        AssigneeName = assigneeName,
                        IsAssignedToCurrentUser =
                            (!string.IsNullOrEmpty(t.AssignedTo) && t.AssignedTo == userId)
                    });
                }

                var vm = new TicketListVM
                {
                    Items = list,
                    IsServiceDesk = isDesk
                };

                // Preserve scope (so buttons/columns render correctly)
                ViewBag.Scope = myScopeOnly ? "My" : "All";
                ViewBag.Query = q; // for showing a "results for" hint and keeping input value
                return View("Index", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tickets.");
                TempData["Error"] = "Failed to search tickets.";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
