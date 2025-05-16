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
using System.IO;

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
        public List<TaskProgressViewModel> TaskProgressList { get; set; } = new List<TaskProgressViewModel>();
        public class TaskProgressViewModel
        {
            public int Id { get; set; }
            public string TaskName { get; set; }
            public string Description { get; set; }
            public string CategoryName { get; set; }
            public string FolderPath { get; set; }
            public int CurrentProgress { get; set; }
            public int TaskAmount { get; set; }
            public DateTime Deadline { get; set; }
            public string Status { get; set; }
            public string CreatedBy { get; set; }

            // Calculate percentage for progress bar
            public int ProgressPercentage => TaskAmount > 0 ? (int)Math.Min(100, (CurrentProgress * 100) / TaskAmount) : 0;

            // Format for display
            public string ProgressText => $"{CurrentProgress}/{TaskAmount}";

            // Progress bar color based on percentage and status
            public string ProgressBarColor
            {
                get
                {
                    if (Status == "Completed") return "bg-success";
                    if (Status == "Overdue") return "bg-danger";
                    if (ProgressPercentage < 30) return "bg-danger";
                    if (ProgressPercentage < 70) return "bg-warning";
                    return "bg-success";
                }
            }
        }

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
            }
            await _context.SaveChangesAsync();

            // Calculate task progress for each task
            TaskProgressList.Clear();
            foreach (var task in await _context.AssignTask
                .Include(t => t.Category)
                .Include(t => t.CreatedByUser)
                .Include(t => t.FolderAccess)
                .Where(t => t.UserId == user.Id)
                .ToListAsync())
            {
                // Construct the folder path based on the task's category
                string folderPath = task.Category?.Name;

                // If there's a FolderAccess record, use its path
                if (task.FolderAccess != null && task.FolderAccess.Category != null)
                {
                    folderPath = task.FolderAccess.Category.Name;

                    // Try to get more specific path if available
                    var area = await _context.Areas
                        .FirstOrDefaultAsync(a => a.CategoryId == task.FolderAccess.CategoryId);
                    if (area != null)
                    {
                        folderPath = $"{folderPath}/{area.Name}";
                    }
                }

                // Count files in the directory
                int currentProgress = 0;
                if (!string.IsNullOrEmpty(folderPath))
                {
                    currentProgress = CountFilesInDirectory(folderPath);
                }

                // Create the view model
                TaskProgressList.Add(new TaskProgressViewModel
                {
                    Id = task.Id,
                    TaskName = task.TaskName,
                    Description = task.Description,
                    CategoryName = task.Category?.Name,
                    FolderPath = folderPath,
                    CurrentProgress = currentProgress,
                    TaskAmount = task.TaskAmount ?? 100, // Default to 100 if not set
                    Deadline = task.Deadline,
                    Status = task.Status,
                    CreatedBy = task.CreatedByUser?.UserName
                });
            }
        }

        private int CountFilesInDirectory(string relativePath)
        {
            try
            {
                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");
                string fullPath = Path.Combine(rootPath, relativePath);

                if (!Directory.Exists(fullPath))
                {
                    return 0;
                }

                // Count files directly in this directory
                int count = Directory.GetFiles(fullPath).Length;

                // Count files in subdirectories
                foreach (var dir in Directory.GetDirectories(fullPath))
                {
                    count += CountFilesInSubDirectory(dir);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error counting files: {ex.Message}");
                return 0;
            }
        }

        private int CountFilesInSubDirectory(string path)
        {
            int count = 0;

            // Count files directly in this directory
            count += Directory.GetFiles(path).Length;

            // Count files in subdirectories
            foreach (var dir in Directory.GetDirectories(path))
            {
                count += CountFilesInSubDirectory(dir);
            }

            return count;
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