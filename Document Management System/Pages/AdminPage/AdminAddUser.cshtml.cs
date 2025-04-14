using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Document_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminMnewModel : PageModel
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AdminMnewModel> _logger;

        public AdminMnewModel(UserManager<Users> userManager, ILogger<AdminMnewModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [Phone]
            public string PhoneNumber { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                return Page();
            }

            var user = new Users
            {
                UserName = Input.Email,
                Email = Input.Email,
                fullName = Input.FullName
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                return RedirectToPage("/AdminPage/AdminMall");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error creating user: {Error}", error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}

