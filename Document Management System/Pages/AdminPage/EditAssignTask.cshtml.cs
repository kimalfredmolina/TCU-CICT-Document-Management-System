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
    public class EditAssignTaskModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public EditAssignTaskModel(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Category> Categories { get; set; }
        public List<Users> UsersWithAccess { get; set; }

        [BindProperty]
        public TaskAssignmentInput Input { get; set; }

        public class TaskAssignmentInput
        {
            public int TaskId { get; set; }

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
            public DateTime StartDate { get; set; }

            [Required(ErrorMessage = "Please select a deadline")]
            public DateTime Deadline { get; set; }

            [Required(ErrorMessage = "Please select a status")]
            public string Status { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var task = await _context.AssignTask
                .Include(t => t.FolderAccess)
                .ThenInclude(fa => fa.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                TempData["ErrorMessage"] = "Task not found.";
                return RedirectToPage("/AdminPage/AdminAssignTask");
            }


            // Load all categories
            Categories = await _context.Categories.ToListAsync();

            // Load users with access to the task's folder
            UsersWithAccess = await _context.FolderAccess
                .Where(fa => fa.CategoryId == task.CategoryId)
                .Include(fa => fa.User)
                .Select(fa => fa.User)
                .Distinct()
                .ToListAsync();

            // Populate the input model
            Input = new TaskAssignmentInput
            {
                TaskId = task.Id,
                CategoryId = task.CategoryId,
                UserId = task.UserId,
                TaskName = task.TaskName,
                Description = task.Description,
                StartDate = task.StartDate.ToLocalTime(),
                Deadline = task.Deadline.ToLocalTime(),
                Status = task.Status
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload necessary data if validation fails
                Categories = await _context.Categories.ToListAsync();
                UsersWithAccess = await _context.FolderAccess
                    .Where(fa => fa.CategoryId == Input.CategoryId)
                    .Include(fa => fa.User)
                    .Select(fa => fa.User)
                    .Distinct()
                    .ToListAsync();

                return Page();
            }

            var task = await _context.AssignTask.FindAsync(Input.TaskId);
            if (task == null)
            {
                TempData["ErrorMessage"] = "Task not found.";
                return RedirectToPage("/AdminPage/AdminAssignTask");
            }

            // Verify the user has access to the selected folder
            var folderAccess = await _context.FolderAccess
                .FirstOrDefaultAsync(fa => fa.CategoryId == Input.CategoryId && fa.UserId == Input.UserId);

            if (folderAccess == null)
            {
                TempData["ErrorMessage"] = "The selected user doesn't have access to this folder.";
                return Page();
            }

            try
            {
                // Update task properties
                task.CategoryId = Input.CategoryId;
                task.UserId = Input.UserId;
                task.TaskName = Input.TaskName;
                task.Description = Input.Description;
                task.StartDate = Input.StartDate.ToUniversalTime();
                task.Deadline = Input.Deadline.ToUniversalTime();
                task.Status = Input.Status;
                task.FolderAccessId = folderAccess.Id;

                _context.AssignTask.Update(task);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Task has been successfully updated.";
                return RedirectToPage("/AdminPage/AdminAssignTask");
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while saving: {ex.InnerException?.Message ?? ex.Message}";
                return Page();
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
    }
}