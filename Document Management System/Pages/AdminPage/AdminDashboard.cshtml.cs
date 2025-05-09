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
using System;

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
        public int TotalAssignedTasksCount { get; set; }
        public int TotalDocumentsCount { get; private set; }
        public Dictionary<string, int> FolderAssignmentsCount { get; private set; }
        public Dictionary<string, int> AreasByCategoryCount { get; private set; }
        public List<TaskViewModel> UserTasks { get; set; } = new List<TaskViewModel>();

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
            TotalDocumentsCount = await _context.Documents.CountAsync();
            TotalAssignedTasksCount = await _context.AssignTask.CountAsync();

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

            UserTasks = await _context.AssignTask
                .Include(t => t.Category)  // This loads the Category relationship
                .Include(t => t.CreatedByUser)
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.Deadline)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    CategoryName = t.Category.Name,  // Accessing Category name directly
                    Deadline = t.Deadline,
                    Status = t.Status,
                    CreatedBy = t.CreatedByUser.UserName
                })
                .ToListAsync();

            // Check for overdue tasks
            foreach (var task in UserTasks.Where(t => t.Deadline < DateTime.Now && t.Status != "Completed"))
            {
                task.Status = "Overdue";
                _context.Update(task);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostUpdateTaskStatusAsync(int taskId, string status)
        {
            try
            {
                var task = await _context.AssignTask
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    return new JsonResult(new { success = false, message = "Task not found" });
                }

                // Update the status
                task.Status = status;
                _context.Update(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Task {taskId} status updated to {status}");
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating task status: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}