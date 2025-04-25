using Document_Management_System.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Document_Management_System.Data;

namespace Document_Management_System.Pages.AdminPage
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminDashboardModel : PageModel
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AdminDashboardModel> _logger;
        private readonly AppDbContext _context; // Add this for database access

        public string UserRole { get; private set; }
        public int ActiveUsersCount { get; private set; }
        public int CategoriesCount { get; private set; } // Add this property

        public AdminDashboardModel(
            UserManager<Users> userManager,
            ILogger<AdminDashboardModel> logger,
            AppDbContext context) // Add this parameter
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
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

            ActiveUsersCount = _userManager.Users.Count();

            // Count categories from the database
            CategoriesCount = await _context.Categories.CountAsync();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }
    }
}