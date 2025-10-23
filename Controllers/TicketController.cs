using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Services;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels.Employee;
using System.Security.Claims;
using NoSQL_Project.ViewModels.Tickets;

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




        // GET: /Ticket
        // GET: /Ticket → "My Tickets"
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if current user is ServiceDesk or normal employee
            var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ServiceDesk → tickets assigned to them
            // Normal user → tickets they reported
            IEnumerable<Ticket> tickets = isDesk
                ? await _ticketService.GetAssignedToUserAsync(userId)  // new method you added
                : await _ticketService.GetForUserAsync(userId);

            // Map database tickets to view model items
            var list = tickets.Select(t => new TicketListItemVM
            {
                Id = t.Id!,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                Deadline = t.Deadline,
                ReporterName = "(hidden)",
                AssigneeName = null  // if exists, otherwise null/"-"t.AssignedToDisplayName
            });

            // Prepare view model
            var vm = new TicketListVM
            {
                Items = list,
                IsServiceDesk = isDesk
            };

            // "My" flag for the view title
            ViewBag.Scope = "My";

            return View("Index", vm);
        }
        // GET: /Ticket/All  → "All Tickets" (ServiceDesk only)
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

            var list = tickets.Select(t => new TicketListItemVM
            {
                Id = t.Id!,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                Deadline = t.Deadline,
                ReporterName = "(hidden)",
                AssigneeName = null // اگر نمایش اسم لازم شد بعداً map می‌کنیم
            });

            var vm = new TicketListVM
            {
                Items = list,
                IsServiceDesk = true
            };

            ViewBag.Scope = "All";
            return View("Index", vm); // از همان ویوی Index استفاده می‌کنیم
        }


        // GET: /Ticket/Details/{id}
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
                AssigneeName = null
            };
            return View(vm);
        }

        // GET: /Ticket/Create
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

        // POST: /Ticket/Create
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

            if (isDesk && string.IsNullOrEmpty(ticket.ReportedBy))
            {
                ModelState.AddModelError(nameof(vm.ReportedBy), "Please select the reporter.");
                var employees = await _employeeService.GetListAsync();
                vm.ReporterOptions = employees.Select(e =>
                    new SelectListItem(GetDisplayName(e), e.Id));
                return View(vm);
            }

            await _ticketService.CreateTicketAsync(ticket);
            TempData["Success"] = "Ticket created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Ticket/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            // چک کردن ورودی برای null
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid ticket ID.";
                return RedirectToAction("Index");
            }

            // فراخوانی تیکت بر اساس ID از سرویس (متد درست = GetTicketByIdAsync)
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("Index");
            }

            // ساخت ViewModel برای ویرایش
            var model = new TicketEditVM
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority.ToString(),
                Status = ticket.Status.ToString(),

                // اگر مدل Ticket شامل HandlingInfo است، باید اینجا بررسی شود
                // چون ممکن است داخلش Id به‌صورت مستقیم وجود نداشته باشد.
                // در اینجا فرض شده که HandlingInfo شامل EmployeeId است.
                HandledById = ticket.HandledBy?.FirstOrDefault()?.EmployeeId
            };

            // فقط نقش ServiceDesk حق دارد Dropdown مربوط به "Assign To" را ببیند
            var role = HttpContext.Session.GetString("Role");
            if (role == "ServiceDesk")
            {
                // متد درست برای گرفتن لیست کارمندان
                var employees = await _employeeService.GetListAsync();

                // پر کردن DropDown با نام و Id کارمندان
                model.EmployeeOptions = employees.Select(e => new SelectListItem
                {
                    Value = e.Id,
                    Text = $"{e.Name.FirstName} {e.Name.LastName}"
                }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketEditVM model)
        {
            // بررسی اعتبار داده‌ها
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return View(model);
            }

            // واکشی تیکت موجود از دیتابیس
            var ticket = await _ticketService.GetTicketByIdAsync(model.Id);
            if (ticket == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("Index");
            }

            // بروزرسانی فیلدهای اصلی تیکت با داده‌های جدید از فرم
            ticket.Title = model.Title;
            ticket.Description = model.Description;
            ticket.Priority = Enum.Parse<TicketPriority>(model.Priority);
            ticket.Status = Enum.Parse<TicketStatus>(model.Status);

            // اگر نقش ServiceDesk باشد، کاربر می‌تواند فرد جدیدی را Assign کند
            var role = HttpContext.Session.GetString("Role");
            if (role == "ServiceDesk" && !string.IsNullOrEmpty(model.HandledById))
            {
                // فراخوانی سرویس برای واکشی اطلاعات کارمند مورد نظر
                var emp = await _employeeService.GetDetailsAsync(model.HandledById);

                if (emp != null)
                {
                    // ساخت لیست جدید برای فیلد HandledBy
                    // این کار باعث می‌شود کاربر انتخاب‌شده مسئول این تیکت شود
                    ticket.HandledBy = new List<HandlingInfo>
{
    new HandlingInfo { EmployeeId = emp.Id }
};
                }
            }

            // ذخیره تغییرات در پایگاه داده (متد درست = UpdateTicketAsync)
            await _ticketService.UpdateTicketAsync(ticket.Id, ticket);

            TempData["Success"] = "Ticket updated successfully!";
            return RedirectToAction("Index");
        }


        // GET: /Ticket/Delete/{id}
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


            await _ticketService.DeleteTicketAsync(id);
            TempData["Success"] = "Ticket deleted.";
            return RedirectToAction(nameof(Index));
        }

        // helper
        private static string GetDisplayName(EmployeeListViewModel e)
        {
            var fullName = $"{e.Name?.FirstName} {e.Name?.LastName}".Trim();
            return !string.IsNullOrWhiteSpace(fullName) ? $"{fullName} ({e.Email})" : (e.Email ?? e.Id);
        }


        [HttpPost]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToMe(string id, string? returnTo = null)
        {
            // validate id
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

            // keep user in the same list if we passed returnTo=All
            if (string.Equals(returnTo, "All", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction(nameof(All));

            return RedirectToAction(nameof(Index)); // default: My Tickets
        }


        // اگر خواستی بعداً برای انتخاب فرد دیگر هم بگذاری:
        [HttpPost]
        [Authorize(Roles = nameof(RoleType.ServiceDesk))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(string id, string assigneeId)
        {
            if (string.IsNullOrWhiteSpace(id) || !MongoDB.Bson.ObjectId.TryParse(id, out _))
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

    }
}


