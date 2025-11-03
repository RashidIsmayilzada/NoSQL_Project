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
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(ITicketService ticketService, IEmployeeService employeeService, ILogger<TicketController> logger)
        {
            _ticketService = ticketService;
            _employeeService = employeeService;
            _logger = logger;
        }

        // -------------------- INDEX (My Tickets) --------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IEnumerable<Ticket> tickets = isDesk
                ? await _ticketService.GetAssignedToUserAsync(userId)
                : await _ticketService.GetForUserAsync(userId);

            var list = new List<TicketListItemVM>();

            foreach (var t in tickets)
            {
                string? assigneeName = null;

                if (t.HandledBy != null && t.HandledBy.Any())
                {
                    var emp = await _employeeService.GetDetailsAsync(t.HandledBy.First().EmployeeId);
                    if (emp != null)
                        assigneeName = $"{emp.Name.FirstName} {emp.Name.LastName}";
                }

                list.Add(new TicketListItemVM
                {
                    Id = t.Id!,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    Deadline = t.Deadline,
                    ReporterName = "(hidden)",
                    AssigneeName = assigneeName ?? "-"
                });
            }

            var vm = new TicketListVM
            {
                Items = list,
                IsServiceDesk = isDesk
            };

            ViewBag.Scope = "My";
            return View("Index", vm);
        }

        // -------------------- ALL (ServiceDesk only) --------------------
        [HttpGet]
        public async Task<IActionResult> All()
        {
            var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
            if (!isDesk)
            {
                TempData["Error"] = "Only ServiceDesk can view all tickets.";
                return RedirectToAction(nameof(Index));
            }

            var tickets = await _ticketService.GetAllTicketsAsync();
            var list = new List<TicketListItemVM>();

            foreach (var t in tickets)
            {
                string? assigneeName = null;

                if (t.HandledBy != null && t.HandledBy.Any())
                {
                    var emp = await _employeeService.GetDetailsAsync(t.HandledBy.First().EmployeeId);
                    if (emp != null)
                        assigneeName = $"{emp.Name.FirstName} {emp.Name.LastName}";
                }

                list.Add(new TicketListItemVM
                {
                    Id = t.Id!,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    Deadline = t.Deadline,
                    ReporterName = "(hidden)",
                    AssigneeName = assigneeName ?? "-"
                });
            }

            var vm = new TicketListVM
            {
                Items = list,
                IsServiceDesk = true
            };

            ViewBag.Scope = "All";
            return View("Index", vm);
        }

        // -------------------- DETAILS --------------------
        [HttpGet]
        public async Task<IActionResult> Details(string id)
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

            string? assigneeName = null;
            if (t.HandledBy != null && t.HandledBy.Any())
            {
                var emp = await _employeeService.GetDetailsAsync(t.HandledBy.First().EmployeeId);
                if (emp != null)
                    assigneeName = $"{emp.Name.FirstName} {emp.Name.LastName}";
            }

            var vm = new TicketDetailsVM
            {
                Id = t.Id!,
                Title = t.Title,
                Type = t.Type,
                Priority = t.Priority,
                Deadline = t.Deadline,
                Description = t.Description,
                Status = t.Status,
                ReporterName = "(hidden)",
                AssigneeName = assigneeName ?? "-"
            };

            return View(vm);
        }

        // -------------------- CREATE --------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
            var vm = new TicketCreateVM();

            if (isDesk)
            {
                var employees = await _employeeService.GetListAsync();
                vm.ReporterOptions = employees.Select(e =>
                    new SelectListItem(GetDisplayName(e), e.Id));
            }
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "ServiceDesk,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateVM vm)
        {
            var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                if (isDesk)
                {
                    var employees = await _employeeService.GetListAsync();
                    vm.ReporterOptions = employees.Select(e =>
                        new SelectListItem(GetDisplayName(e), e.Id));
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
        // GET: /Ticket/Edit/{id}
        [HttpGet]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))] // Only ServiceDesk can open the edit form
        public async Task<IActionResult> Edit(string id)
        {
            // Validate id format (Mongo ObjectId)
            if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
            {
                TempData["Error"] = "Invalid ticket ID.";
                return RedirectToAction(nameof(Index));
            }

            // Load ticket from service
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map model to the edit view-model
            var model = new TicketEditVM
            {
                Id = ticket.Id!,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority.ToString(), // enum -> string for dropdown
                Status = ticket.Status.ToString(),     // enum -> string for dropdown
                                                       // If you store handling history, preselect the first handler if present
                HandledById = ticket.HandledBy?.FirstOrDefault()?.EmployeeId
            };

            // Only ServiceDesk sees the "Assign To" dropdown
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

        // POST: /Ticket/Edit
        [HttpPost]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))] // Only ServiceDesk can submit edits
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketEditVM model)
        {
            // Server-side validation
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                // Rebuild dropdown if needed
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

            // Load the existing ticket
            var ticket = await _ticketService.GetTicketByIdAsync(model.Id);
            if (ticket == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map editable fields back to the domain model
            ticket.Title = model.Title;
            ticket.Description = model.Description;
            ticket.Priority = Enum.Parse<TicketPriority>(model.Priority);
            ticket.Status = Enum.Parse<TicketStatus>(model.Status);

            // Update assignee if ServiceDesk selected someone
            if (User.IsInRole(nameof(RoleType.ServiceDesk)) && !string.IsNullOrEmpty(model.HandledById))
            {
                ticket.HandledBy = new List<HandlingInfo>
        {
            new HandlingInfo { EmployeeId = model.HandledById }
        };
            }

            // Persist changes
            await _ticketService.UpdateTicketAsync(ticket.Id!, ticket);

            TempData["Success"] = "Ticket updated successfully!";
            return RedirectToAction(nameof(Index));
        }


        // -------------------- ASSIGN --------------------
        [HttpGet]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        public async Task<IActionResult> Assign(string id)
        {
            var employees = await _employeeService.GetListAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "Name.FirstName");
            ViewBag.TicketId = id;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(string id, string assigneeId)
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

        [HttpPost]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToMe(string id, string? returnTo = null)
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

        // -------------------- DELETE --------------------
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
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

            var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!isDesk && t.ReportedBy != currentUserId)
            {
                TempData["Error"] = "You are not allowed to delete this ticket.";
                return RedirectToAction(nameof(Index));
            }

            return View(t);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out _))
            {
                TempData["Error"] = "Invalid ticket id.";
                return RedirectToAction(nameof(Index));
            }

            await _ticketService.DeleteTicketAsync(id);
            TempData["Success"] = "Ticket deleted.";
            return RedirectToAction(nameof(Index));
        }

        // -------------------- Helper --------------------
        private static string GetDisplayName(EmployeeListViewModel e)
        {
            var fullName = $"{e.Name?.FirstName} {e.Name?.LastName}".Trim();
            return !string.IsNullOrWhiteSpace(fullName)
                ? $"{fullName} ({e.Email})"
                : (e.Email ?? e.Id);
        }
    }
}
