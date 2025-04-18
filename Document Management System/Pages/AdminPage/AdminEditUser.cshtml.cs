using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Document_Management_System.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminEditUserModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<Users> _userManager;

        public AdminEditUserModel(IConfiguration configuration,
                                IWebHostEnvironment environment,
                                UserManager<Users> userManager)
        {
            _configuration = configuration;
            _environment = environment;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public Microsoft.AspNetCore.Http.IFormFile ProfileImage { get; set; }

        public string UserProfileImage { get; set; }
        public string UserId { get; set; }

        public class InputModel
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            UserId = id;

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.fullName
            };

            if (user.ProfileImage != null)
            {
                UserProfileImage = $"data:image/png;base64,{Convert.ToBase64String(user.ProfileImage)}";
            }
            else
            {
                UserProfileImage = null;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            if (!string.IsNullOrEmpty(Input.Email))
            {
                user.Email = Input.Email;
            }

            if (!string.IsNullOrEmpty(Input.UserName))
            {
                user.UserName = Input.UserName;
            }

            user.PhoneNumber = Input.PhoneNumber;
            user.fullName = Input.FullName;

            if (!string.IsNullOrEmpty(Input.Password) && !string.IsNullOrEmpty(Input.ConfirmPassword))
            {
                if (Input.Password != Input.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "The password and confirmation password do not match.");
                    return Page();
                }

                // Remove the current password
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    foreach (var error in removePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                // Add the new password
                var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.Password);
                if (!addPasswordResult.Succeeded)
                {
                    foreach (var error in addPasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            // Handle profile image upload
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ProfileImage.CopyToAsync(memoryStream);
                    user.ProfileImage = memoryStream.ToArray();
                }
            }

            // Update all user properties at once
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            return RedirectToPage("/AdminPage/AdminUserTable");
        }
    }
}