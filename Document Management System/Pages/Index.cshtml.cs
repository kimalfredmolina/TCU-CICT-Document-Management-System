using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Document_Management_System.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Retrieve the user
                var user = await _signInManager.UserManager.FindByNameAsync(Input.Username);

                if (user != null)
                {
                    // Get roles for the user
                    var roles = await _signInManager.UserManager.GetRolesAsync(user);

                    // Create claims for the user
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("FullName", user.fullName ?? string.Empty) // Custom claim for full name
            };

                    // Add role claims
                    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                    // Create a ClaimsIdentity
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Create a ClaimsPrincipal
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Sign in the user with the claims
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
                    {
                        IsPersistent = true, // Set to true if you want the cookie to persist across sessions
                        ExpiresUtc = DateTime.UtcNow.AddHours(1) // Set cookie expiration
                    });

                    // Redirect based on role
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToPage("/AdminPage/AdminDashboard");
                    }
                    else if (roles.Contains("Staff"))
                    {
                        return RedirectToPage("/AdminPage/AdminDashboard");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "User does not have proper roles.");
                        return Page();
                    }
                }
            }

            // If login fails, return the page with an error message
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }



    }
}