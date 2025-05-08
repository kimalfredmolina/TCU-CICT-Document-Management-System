using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Document_Management_System.Data;
using Microsoft.AspNetCore.Identity;
using Document_Management_System.Models;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Document_Management_System.Pages.AdminPage
{
    [Authorize(Roles = "Admin")]
    public class AdminAssignTaskModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public AdminAssignTaskModel(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Category> Categories { get; set; }
        public List<Users> UsersWithAccess { get; set; } // Only users with folder access
        public List<AssignTask> TaskAssignments { get; set; }

        [BindProperty]
        public TaskAssignmentInput Input { get; set; }

        public class TaskAssignmentInput
        {
            [Required(ErrorMessage = "Please select a folder")]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Please select a user")]
            public string UserId { get; set; }

            [Required(ErrorMessage = "Please enter a task name")]
            [StringLength(100, ErrorMessage = "Task name cannot exceed 100 characters")]
            public string TaskName { get; set; }

            [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
            public string Description { get; set; }

            [Required(ErrorMessage = "Please select a start date")]
            public DateTime StartDate { get; set; } = DateTime.Today;

            [Required(ErrorMessage = "Please select a deadline")]
            public DateTime Deadline { get; set; } = DateTime.Today.AddDays(7);

            [Required(ErrorMessage = "Please select a status")]
            public string Status { get; set; } = "Pending";
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Categories = await _context.Categories.ToListAsync();

            UsersWithAccess = new List<Users>();

            TaskAssignments = await _context.AssignTask
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.CreatedByUser)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var currentUserId = _userManager.GetUserId(User);

            try
            {
                // Verify the user has access to the selected folder
                var folderAccess = await _context.FolderAccess
                    .FirstOrDefaultAsync(fa => fa.CategoryId == Input.CategoryId && fa.UserId == Input.UserId);

                if (folderAccess == null)
                {
                    TempData["ErrorMessage"] = "The selected user doesn't have access to this folder.";
                    await LoadDataAsync();
                    return Page();
                }

                var newTask = new AssignTask
                {
                    FolderAccessId = folderAccess.Id,
                    UserId = Input.UserId, // Add this line
                    TaskName = Input.TaskName,
                    Description = Input.Description,
                    StartDate = Input.StartDate.ToUniversalTime(),
                    Deadline = Input.Deadline.ToUniversalTime(),
                    Status = Input.Status,
                    CreatedDate = DateTime.UtcNow,
                    CreatedByUserId = currentUserId
                };

                _context.AssignTask.Add(newTask);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Task has been successfully assigned.";
                return RedirectToPage();
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while saving: {ex.InnerException?.Message ?? ex.Message}";
                await LoadDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteTaskAsync([FromBody] DeleteTaskRequest request)
        {
            try
            {
                var task = await _context.AssignTask.FindAsync(request.TaskId);
                if (task == null)
                {
                    return new JsonResult(new { success = false, message = "Task not found" });
                }

                _context.AssignTask.Remove(task);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<JsonResult> OnGetUsersWithAccess(int categoryId)
        {
            var users = await _context.FolderAccess
                .Where(fa => fa.CategoryId == categoryId)
                .Include(fa => fa.User)
                .Select(fa => new
                {
                    id = fa.UserId,
                    userName = fa.User.UserName,
                    email = fa.User.Email
                })
                .Distinct()
                .ToListAsync();

            return new JsonResult(users);
        }

        public class DeleteTaskRequest
        {
            public int TaskId { get; set; }
        }
    }
}