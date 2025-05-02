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
    public class AdminSharedModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public AdminSharedModel(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Category> Categories { get; set; }
        public List<Users> Users { get; set; }
        public List<FolderAccess> FolderAccessList { get; set; }

        [BindProperty]
        public FolderAssignmentInput Input { get; set; }

        public class FolderAssignmentInput
        {
            [Required(ErrorMessage = "Please select a folder")]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Please select at least one user")]
            [MinLength(1, ErrorMessage = "Please select at least one user")]
            public List<string> UserIds { get; set; } = new List<string>();
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Categories = await _context.Categories.ToListAsync();
            Users = await _userManager.Users.ToListAsync();
            FolderAccessList = await _context.FolderAccess
                .Include(f => f.Category)
                .Include(f => f.User)
                .Include(f => f.AssignedByUser)
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
                foreach (var userId in Input.UserIds)
                {
                    var existingAssignment = await _context.FolderAccess
                        .FirstOrDefaultAsync(fa => fa.CategoryId == Input.CategoryId && fa.UserId == userId);

                    if (existingAssignment == null)
                    {
                        var newAssignment = new FolderAccess
                        {
                            CategoryId = Input.CategoryId,
                            UserId = userId,
                            AssignedDate = DateTime.UtcNow,
                            AssignedByUserId = currentUserId
                        };
                        _context.FolderAccess.Add(newAssignment);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Folder access has been successfully assigned.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "An error occurred while saving. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAccessAsync([FromBody] DeleteAccessRequest request)
        {
            try
            {
                var access = await _context.FolderAccess.FindAsync(request.AccessId);
                if (access == null)
                {
                    return new JsonResult(new { success = false, message = "Access record not found" });
                }

                _context.FolderAccess.Remove(access);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class DeleteAccessRequest
        {
            public int AccessId { get; set; }
        }
    }
}