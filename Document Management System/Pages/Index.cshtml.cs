using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Document_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Document_Management_System.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SignInManager<Users> _signInManager;

        public IndexModel(SignInManager<Users> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public void OnGet()
        {
            // This method is called when the page is loaded (HTTP GET request).
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Return the page with validation errors
            }

            // Use SignInManager to authenticate the user
            var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Redirect to the admin dashboard upon successful login
                return RedirectToPage("/AdminPage/AdminDashboard");
            }

            // If login fails, display an error message
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }
    }
}