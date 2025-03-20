using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Document_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Document_Management_System.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(SignInManager<Users> signInManager, ILogger<IndexModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }
        public InputModel Input { get; set; }

        public void OnGet()
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
        public void OnGet()
        {
            // This method is called when the page is loaded (HTTP GET request).
        }

            // Add your login logic here
            // For example, validate the username and password
            if (Input.Username == "user" && Input.Password == "user")
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
                _logger.LogInformation("User {Username} logged in successfully.", Input.Username);
                // Redirect to the admin dashboard upon successful login
                return RedirectToPage("/AdminPage/AdminDashboard");
            }
            else
            {
                _logger.LogWarning("Login attempt failed for user {Username}.", Input.Username);
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
