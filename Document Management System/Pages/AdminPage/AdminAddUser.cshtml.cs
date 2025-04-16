using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Document_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminMnewModel : PageModel
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AdminMnewModel> _logger;

        public AdminMnewModel(
            UserManager<Users> userManager,
            ILogger<AdminMnewModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public IFormFile ProfileImage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Full name is required")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone(ErrorMessage = "Invalid phone number")]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Confirm password is required")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
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
                _logger.LogWarning("Model state is invalid. Errors: {Errors}",
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return Page();
            }

            if (!Input.Password.Any(char.IsDigit))
            {
                ModelState.AddModelError("Input.Password", "Password must contain at least one digit (0-9).");
                return Page();
            }

            byte[] profileImageBytes = null;
            if (ProfileImage != null)
            {
                if (ProfileImage.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProfileImage", "The image must be less than 2MB.");
                    return Page();
                }

                using (var memoryStream = new MemoryStream())
                {
                    await ProfileImage.CopyToAsync(memoryStream);
                    profileImageBytes = memoryStream.ToArray();
                }
            }

            var user = new Users
            {
                UserName = Input.Email,
                Email = Input.Email,
                fullName = Input.FullName,
                PhoneNumber = Input.PhoneNumber,
                ProfileImage = profileImageBytes
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password for {Email}.", Input.Email);
                return RedirectToPage("/AdminPage/AdminUserTable");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error creating user: {ErrorCode} - {ErrorDescription}",
                    error.Code, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}