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
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUserTable(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserWithRole> UsersList { get; set; } = new List<UserWithRole>();

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public async Task OnGetAsync()
        {
            var users = _userManager.Users;

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                users = users
                    .Where(u => u.Id.Contains(SearchQuery) ||
                               u.UserName.Contains(SearchQuery) ||
                               u.Email.Contains(SearchQuery));
            }

            UsersList = new List<UserWithRole>();

            foreach (var user in users.ToList())
            {
                var roles = await _userManager.GetRolesAsync(user);
                UsersList.Add(new UserWithRole
                {
                    User = user,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
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

    public class UserWithRole
    {
        public Users User { get; set; }
        public string Role { get; set; }
    }
}