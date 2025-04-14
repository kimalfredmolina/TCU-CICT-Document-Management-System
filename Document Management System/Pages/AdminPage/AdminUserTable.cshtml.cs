using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Document_Management_System.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminMallModel : PageModel
    {
        private readonly UserManager<Users> _userManager;

        public AdminMallModel(UserManager<Users> userManager)
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
                // Filter users based on the search query
                UsersList = users
                    .Where(u => u.Id.Contains(SearchQuery) || u.UserName.Contains(SearchQuery) || u.Email.Contains(SearchQuery))
                    .ToList();
            }
            else
            {
                // Fetch all users if no search query is provided
                UsersList = users.ToList();
            }
        }
    }
}
