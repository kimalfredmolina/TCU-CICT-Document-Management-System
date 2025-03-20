using Microsoft.AspNetCore.Mvc;

namespace Document_Management_System.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
    