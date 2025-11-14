using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.ViewModels.ForgotPassword;

namespace NoSQL_Project.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IPasswordResetTokenService _tokenService;
        private readonly IEmailSenderService _emailSenderService;

        public ForgotPasswordController(IEmployeeService employeeService,
            IPasswordResetTokenService tokenService,
            IEmailSenderService emailSenderService)
        {
            _employeeService = employeeService;
            _tokenService = tokenService;
            _emailSenderService = emailSenderService;
        }


        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

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

            TempData["Info"] = "Check your email, if an account with that email exists, we sent a password reset link.";
            return RedirectToAction("Login", "Login");
        }

        // GET: /Account/ResetPassword?userId=...&token=...
        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest();

            var validatedUserId = _tokenService.ValidateToken(token);
            if (validatedUserId == null || validatedUserId != userId)
            {
                TempData["Error"] = "Invalid or expired password reset link.";
                return RedirectToAction("Login", "Login");
            }

            var vm = new ResetPasswordViewModel
            {
                EmployeeId = userId,
                Token = token
            };

            return View(vm);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var validatedUserId = _tokenService.ValidateToken(model.Token);
            if (validatedUserId == null || validatedUserId != model.EmployeeId)
            {
                TempData["Error"] = "Invalid or expired password reset link.";
                return RedirectToAction("Login","Login");
            }

            var user = await _employeeService.GetEmployeeAsync(model.EmployeeId);
            if (user == null)
            {
                TempData["Error"] = "Could not find user.";
                return RedirectToAction("Login", "Login");
            }

            if (!await _employeeService.ChangePasswordAsync(model.EmployeeId, model.NewPassword, model.ConfirmPassword))
            {
                TempData["Error"] = "Could not change password.";
                return RedirectToAction("Login", "Login");
            }

            TempData["Success"] = "Password reset successful, you can now login.";
            return RedirectToAction("Login", "Login");
        }

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
