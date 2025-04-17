using Document_Management_System.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Document_Management_System.Pages.AdminPage

{
    [Authorize(Roles = "Admin,Staff")] // Allow both Admin and Staff roles

    public class AdminDashboardModel : PageModel
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AdminDashboardModel> _logger;

        public string UserRole { get; private set;   }

        public AdminDashboardModel(UserManager<Users> userManager, ILogger<AdminDashboardModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || !User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("No user is currently logged in or the session is invalid.");
                UserRole = "Not logged in";
                return;
            }

            _logger.LogInformation($"Found user: {user.UserName} ({user.Id})");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count > 0)
            {
                UserRole = roles.First();
                _logger.LogInformation($"User is in role: {UserRole}");
            }
            else
            {
                _logger.LogWarning($"User {user.UserName} has NO ROLES!");
                UserRole = "No role assigned";
            }
        }
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index"); // Or wherever your login page is
        }
    }
}
