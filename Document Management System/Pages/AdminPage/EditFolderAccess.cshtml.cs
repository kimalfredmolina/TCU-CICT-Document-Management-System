using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Document_Management_System.Data;
using Microsoft.AspNetCore.Identity;
using Document_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Document_Management_System.Pages.AdminPage
{
    [Authorize(Roles = "Admin")]
    public class EditFolderAccessModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public EditFolderAccessModel(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public EditFolderAccessInput Input { get; set; }

        public string CurrentFolderName { get; set; }
        public string CurrentUserName { get; set; }
        public string CurrentUserEmail { get; set; }
        public int CurrentCategoryId { get; set; }
        public List<Category> Categories { get; set; }

        public class EditFolderAccessInput
        {
            public int AccessId { get; set; }

            [Required(ErrorMessage = "Please select a folder")]
            public int NewCategoryId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var access = await _context.FolderAccess
                .Include(f => f.Category)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == Id);

            if (access == null)
            {
                return NotFound();
            }

            Input = new EditFolderAccessInput
            {
                AccessId = access.Id
            };

            CurrentFolderName = access.Category.Name;
            CurrentUserName = access.User.UserName;
            CurrentUserEmail = access.User.Email;
            CurrentCategoryId = access.CategoryId;
            Categories = await _context.Categories.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = await _context.Categories.ToListAsync();
                return Page();
            }

            var access = await _context.FolderAccess.FindAsync(Input.AccessId);
            if (access == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            try
            {
                access.CategoryId = Input.NewCategoryId;
                access.AssignedDate = System.DateTime.UtcNow;
                access.AssignedByUserId = currentUserId;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Folder access has been successfully updated.";
                return RedirectToPage("/AdminPage/AdminAssignFolder");
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "An error occurred while saving. Please try again.";
                return Page();
            }
        }
    }
}