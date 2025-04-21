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
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Pages.AdminPage
{
    [Authorize(Roles = "Admin")] // Restrict to Admin only
    public class AdminEditUserModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminEditUserModel(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _configuration = configuration;
            _environment = environment;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public Microsoft.AspNetCore.Http.IFormFile ProfileImage { get; set; }

        public string UserProfileImage { get; set; }
        public string UserId { get; set; }
        public List<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
        public string CurrentUserRole { get; set; }

        public class InputModel
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public string SelectedRole { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Check if current user is admin
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !(await _userManager.IsInRoleAsync(currentUser, "Admin")))
            {
                return Forbid();
            }

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

            // Get current user role
            var userRoles = await _userManager.GetRolesAsync(user);
            CurrentUserRole = userRoles.FirstOrDefault();

            // Populate roles dropdown
            var allRoles = await _roleManager.Roles.ToListAsync();
            RoleList = allRoles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = r.Name == CurrentUserRole
            }).ToList();

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
            // Check if current user is admin
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !(await _userManager.IsInRoleAsync(currentUser, "Admin")))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            // Update basic user info
            user.Email = Input.Email;
            user.UserName = Input.UserName;
            user.PhoneNumber = Input.PhoneNumber;
            user.fullName = Input.FullName;

            // Handle password change if provided
            if (!string.IsNullOrEmpty(Input.Password) && !string.IsNullOrEmpty(Input.ConfirmPassword))
            {
                if (Input.Password != Input.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "The password and confirmation password do not match.");
                    await LoadRoles(user);
                    return Page();
                }

                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    foreach (var error in removePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await LoadRoles(user);
                    return Page();
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.Password);
                if (!addPasswordResult.Succeeded)
                {
                    foreach (var error in addPasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await LoadRoles(user);
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

            // Handle role update
            if (!string.IsNullOrEmpty(Input.SelectedRole))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, Input.SelectedRole);
            }

            // Update user
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadRoles(user);
                return Page();
            }

            return RedirectToPage("/AdminPage/AdminUserTable");
        }

        private async Task LoadRoles(Users user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            CurrentUserRole = userRoles.FirstOrDefault();

            var allRoles = await _roleManager.Roles.ToListAsync();
            RoleList = allRoles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = r.Name == CurrentUserRole
            }).ToList();
        }
    }
}