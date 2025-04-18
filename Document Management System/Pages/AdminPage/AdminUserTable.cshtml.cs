using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Document_Management_System.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminUserTable : PageModel
    {
        private readonly UserManager<Users> _userManager;

        public AdminUserTable(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

        public List<Users> UsersList { get; set; } = new List<Users>();

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public async Task OnGetAsync()
        {
            var users = _userManager.Users;

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                UsersList = users
                    .Where(u => u.Id.Contains(SearchQuery) || u.UserName.Contains(SearchQuery) || u.Email.Contains(SearchQuery))
                    .ToList();
            }
            else
            {
                UsersList = users.ToList();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new JsonResult(new { success = false, message = "No user ID provided" });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return new JsonResult(new { success = true });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Failed to delete user" });
            }
        }
    }
}