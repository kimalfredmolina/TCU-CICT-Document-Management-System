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
using System.Collections.Generic;

namespace Document_Management_System.Pages.AdminPage
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminDashboardModel : PageModel
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AdminDashboardModel> _logger;
        private readonly AppDbContext _context;

        public string UserRole { get; private set; }
        public int ActiveUsersCount { get; private set; }
        public int CategoriesCount { get; private set; }
        public int AreasCount { get; private set; }
        public int TotalDocumentsCount { get; private set; } // New property for document count
        public Dictionary<string, int> FolderAssignmentsCount { get; private set; }
        public Dictionary<string, int> AreasByCategoryCount { get; private set; }

        public AdminDashboardModel(
            UserManager<Users> userManager,
            ILogger<AdminDashboardModel> logger,
            AppDbContext context)
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
            CategoriesCount = await _context.Categories.CountAsync();
            AreasCount = await _context.Areas.CountAsync();

            // Count all documents in the Documents table
            TotalDocumentsCount = await _context.Documents.CountAsync();

            FolderAssignmentsCount = await _context.FolderAccess
                .Include(f => f.Category)
                .GroupBy(f => f.Category.Name)
                .Select(g => new { FolderName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.FolderName, x => x.Count);

            AreasByCategoryCount = await _context.Areas
                .Include(a => a.Category)
                .GroupBy(a => a.Category.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryName, x => x.Count);
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }
    }
}