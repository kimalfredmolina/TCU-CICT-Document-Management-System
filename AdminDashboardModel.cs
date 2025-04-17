using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Document_Management_System.Pages.AdminPage
{
    public class AdminDashboardModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AdminDashboardModel> _logger;

        public AdminDashboardModel(UserManager<IdentityUser> userManager, ILogger<AdminDashboardModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Count > 0)
                {
                    foreach (var role in roles)
                    {
                        _logger.LogInformation($"Role fetched for user {user.UserName}: {role}");
                    }
                }
                else
                {
                    _logger.LogWarning($"No roles assigned to user {user.UserName}.");
                }
            }
            else
            {
                _logger.LogError("No user found for the current session.");
            }
        }
    }
}
