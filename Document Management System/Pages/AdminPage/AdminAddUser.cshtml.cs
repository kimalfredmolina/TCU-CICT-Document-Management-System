using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Document_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Document_Management_System.Pages.AdminPage
{
    //[Authorize(Roles = "Admin")]
    public class AdminMnewModel : PageModel
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminMnewModel> _logger;

        public AdminMnewModel(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminMnewModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public IFormFile ProfileImage { get; set; }

        public List<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();

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

            [Required(ErrorMessage = "Role is required")]
            [Display(Name = "Role")]
            public string SelectedRole { get; set; }
        }

        public async Task OnGetAsync()
        {
            // Populate roles dropdown
            var roles = _roleManager.Roles.ToList();
            RoleList = roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Repopulate roles dropdown if returning to page
                await OnGetAsync();
                return Page();
            }

            // Password complexity check
            if (!Input.Password.Any(char.IsDigit))
            {
                ModelState.AddModelError("Input.Password", "Password must contain at least one digit (0-9).");
                await OnGetAsync();
                return Page();
            }

            // Handle profile image
            byte[] profileImageBytes = null;
            if (ProfileImage != null)
            {
                if (ProfileImage.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProfileImage", "The image must be less than 2MB.");
                    await OnGetAsync();
                    return Page();
                }

                using (var memoryStream = new MemoryStream())
                {
                    await ProfileImage.CopyToAsync(memoryStream);
                    profileImageBytes = memoryStream.ToArray();
                }
            }

            // Create user
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

                // Add selected role to user
                if (!string.IsNullOrEmpty(Input.SelectedRole))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, Input.SelectedRole);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Failed to add role {Role} to user {Email}. Errors: {Errors}",
                            Input.SelectedRole, Input.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));

                        // Optionally handle role assignment failure (e.g., show error or delete user)
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Role assignment failed: {error.Description}");
                        }

                        // Delete user if role assignment failed (optional)
                        await _userManager.DeleteAsync(user);
                        await OnGetAsync();
                        return Page();
                    }
                }

                return RedirectToPage("/AdminPage/AdminUserTable");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error creating user: {ErrorCode} - {ErrorDescription}",
                    error.Code, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await OnGetAsync();
            return Page();
        }
    }
}