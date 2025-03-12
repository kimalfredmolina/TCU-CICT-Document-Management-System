using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Document_Management_System.Pages.AdminPage
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public required InputModel Input { get; set; }

        public class InputModel
        {
            public required string Username { get; set; }
            public required string Password { get; set; }
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            // Example hardcoded credentials for demonstration purposes
            const string validUsername = "admin";
            const string validPassword = "password123";

            if (Input.Username == validUsername && Input.Password == validPassword)
            {
                // Redirect to the admin dashboard or another page upon successful login
                return RedirectToPage("/AdminPage/AdminDashboard");
            }

            // If login fails, display an error message
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }
    }
}