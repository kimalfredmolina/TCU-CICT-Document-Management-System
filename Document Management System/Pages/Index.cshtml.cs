using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Document_Management_System.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public LoginInputModel Input { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Add your login logic here
            // For example, validate the username and password
            if (Input.Username == "user" && Input.Password == "user")
            {
                return RedirectToPage("/AdminPage/AdminDashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        public class LoginInputModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
