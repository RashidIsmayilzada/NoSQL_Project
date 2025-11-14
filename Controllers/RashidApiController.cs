using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels.Dashboard;
using NoSQL_Project.ViewModels.Employee;
using NoSQL_Project.ViewModels.ForgotPassword;
using NoSQL_Project.ViewModels.Tickets;
using System.Security.Claims;

namespace NoSQL_Project.Controllers;

// REST API controller containing 1 GET and 1 POST method from every other controller
[ApiController]
[Route("api/rashid")]
public class RashidApiController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ITicketService _ticketService;
    private readonly IDashboardService _dashboardService;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly ILogger<RashidApiController> _logger;

    // Constructor with dependency injection
    public RashidApiController(
        IEmployeeService employeeService,
        ITicketService ticketService,
        IDashboardService dashboardService,
        IPasswordResetTokenService tokenService,
        IEmailSenderService emailSenderService,
        ILogger<RashidApiController> logger)
    {
        _employeeService = employeeService;
        _ticketService = ticketService;
        _dashboardService = dashboardService;
        _tokenService = tokenService;
        _emailSenderService = emailSenderService;
        _logger = logger;
    }

    // GET: api/rashid/dashboard
    [HttpGet("dashboard")]
    [Authorize]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isServiceDesk = role == RoleType.ServiceDesk.ToString();

            var scope = role == RoleType.ServiceDesk.ToString()
                ? DashboardScope.AllTickets
                : DashboardScope.MyTickets;

            var breakdown = await _dashboardService.GetStatusBreakdownAsync(scope, userId);
            var (total, open, overdue) = await _dashboardService.GetOpenAndOverdueAsync(scope, userId);

            return Ok(new
            {
                isServiceDesk = isServiceDesk,
                statusBreakdown = breakdown,
                totalTickets = total,
                openTickets = open,
                overdueOpen = overdue
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            return StatusCode(500, new { error = "Failed to retrieve dashboard data" });
        }
    }
    
    // GET: api/rashid/employees
    // Returns list of all employees
    [HttpGet("employees")]
    [Authorize(Roles = "ServiceDesk")]
    public async Task<IActionResult> GetEmployees()
    {
        try
        {
            _logger.LogInformation("API: Retrieving all employees");
            var employees = await _employeeService.GetListAsync();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Database error while retrieving employees");
            return StatusCode(500, new { error = "Unable to retrieve employees" });
        }
    }

    // POST: api/rashid/employees
    // Creates a new employee
    [HttpPost("employees")]
    [Authorize(Roles = "ServiceDesk")]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateViewModel employee)
    {
        try
        {
            if (employee == null)
            {
                _logger.LogWarning("API: Create called with null employee");
                return BadRequest(new { error = "Employee data is required" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("API: Invalid model state for employee creation");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("API: Creating new employee");
            await _employeeService.CreateAsync(employee);

            return Ok(new { message = "Employee created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error while creating employee");
            return StatusCode(500, new { error = "Unable to create employee" });
        }
    }

    // GET: api/rashid/forgot-password
    // Returns a message about forgot password functionality
    [HttpGet("forgot-password")]
    public IActionResult GetForgotPassword()
    {
        return Ok(new
        {
            message = "POST your email to this endpoint to receive a password reset link",
            endpoint = "/api/rashid/forgot-password",
            method = "POST",
            requiredFields = new { email = "string" }
        });
    }

    // POST: api/rashid/forgot-password
    // Sends password reset email
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _employeeService.GetEmployeeByEmailAsync(model.Email);

            if (user != null)
            {
                var token = _tokenService.GenerateToken(user.Id.ToString());

                var callbackUrl = Url.Action(
                    action: "ResetPassword",
                    controller: "ForgotPassword",
                    values: new { userId = user.Id.ToString(), token },
                    protocol: Request.Scheme);

                var body = $@"
                            <p>You requested a password reset.</p>
                            <p>Click <a href=""{callbackUrl}"">here</a> to reset your password.</p>
                            <p>If you did not request this, you can ignore this email.</p>";

                await _emailSenderService.SendAsync(model.Email, "Reset your password", body);
            }

            return Ok(new
            {
                message = "If an account with that email exists, we sent a password reset link"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error in forgot password");
            return StatusCode(500, new { error = "Failed to process forgot password request" });
        }
    }

    // GET: api/rashid/login
    // Returns login endpoint information
    [HttpGet("login")]
    public IActionResult GetLogin()
    {
        return Ok(new
        {
            message = "POST your credentials to this endpoint to authenticate",
            endpoint = "/api/rashid/login",
            method = "POST",
            requiredFields = new { email = "string", password = "string" }
        });
    }

    // POST: api/rashid/login
    // Authenticates a user
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel loginModel)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _employeeService.AuthenticateAsync(loginModel.Email, loginModel.Password);
            if (employee == null)
            {
                return Unauthorized(new { error = "Incorrect email or password" });
            }

            return Ok(new
            {
                message = "Authentication successful",
                employee = new
                {
                    id = employee.Id,
                    name = $"{employee.Name.FirstName} {employee.Name.LastName}",
                    email = employee.ContactInfo.Email,
                    role = employee.Role.ToString()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error during login");
            return StatusCode(500, new { error = "Authentication failed" });
        }
    }

    // GET: api/rashid/tickets
    // Returns list of tickets for the current user
    [HttpGet("tickets")]
    [Authorize]
    public async Task<IActionResult> GetTickets()
    {
        try
        {
            var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tickets = isDesk
                ? await _ticketService.GetAssignedToUserAsync(userId)
                : await _ticketService.GetForUserAsync(userId);

            var ticketList = new List<object>();
            foreach (var t in tickets)
            {
                var assigneeName = await ResolveAssigneeNameAsync(t);
                var reporterName = await ResolveReporterNameAsync(t);

                ticketList.Add(new
                {
                    id = t.Id,
                    title = t.Title,
                    status = t.Status.ToString(),
                    priority = t.Priority.ToString(),
                    deadline = t.Deadline,
                    reporterName = reporterName,
                    assigneeName = assigneeName,
                    isAssignedToCurrentUser = (!string.IsNullOrEmpty(t.AssignedTo) && t.AssignedTo == userId)
                });
            }

            return Ok(new
            {
                isServiceDesk = isDesk,
                tickets = ticketList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error loading tickets");
            return StatusCode(500, new { error = "Failed to load tickets" });
        }
    }

    // POST: api/rashid/tickets
    // Creates a new ticket
    [HttpPost("tickets")]
    [Authorize]
    public async Task<IActionResult> CreateTicket([FromBody] TicketCreateVM vm)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isDesk = User.IsInRole(nameof(RoleType.ServiceDesk));
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            return Ok(new
            {
                message = "Ticket created successfully",
                ticketId = ticket.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error creating ticket");
            return StatusCode(500, new { error = "Failed to create ticket" });
        }
    }

    // Helper method to resolve assignee name from ticket
    private async Task<string?> ResolveAssigneeNameAsync(Ticket t)
    {
        var assigneeId = t.AssignedTo;
        if (string.IsNullOrWhiteSpace(assigneeId)) return null;

        var emp = await _employeeService.GetEmployeeAsync(assigneeId);
        return emp == null ? null : $"{emp.Name.FirstName} {emp.Name.LastName}";
    }

    // Helper method to resolve reporter name from ticket
    private async Task<string?> ResolveReporterNameAsync(Ticket t)
    {
        var reporterId = t.ReportedBy;
        if (string.IsNullOrWhiteSpace(reporterId)) return null;

        var emp = await _employeeService.GetEmployeeAsync(reporterId);
        return emp == null ? null : emp.Name.ToString();
    }
}
